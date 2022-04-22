using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using Mzr.Share.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


namespace Mzr.Share.Repositories.Bilibili.Web
{
    public class WebBiliReplyRepository
    {
        private static readonly Uri threadUri = new Uri(@"http://api.bilibili.com/x/v2/reply/main");
        private static readonly Uri replyUri = new Uri(@"http://api.bilibili.com/x/v2/reply/reply");
        private static readonly Dictionary<int, int> threadMap = new Dictionary<int, int>()
        {
            {1, 17 },
            {2, 11 },
            {4, 17 },
            {8, 1 },
            {64, 12 },
            {256, 14 },
            {2048,17 }
        };
        private static Dictionary<string, string> headers = new() { { "referer", "https://t.bilibili.com/" } };
        private static Regex pattern = new(@"jQuery\d*_\d*\((.*)\)", RegexOptions.Compiled);
        private const string baseQuery = @"callback=jQuery3310583964485019705_1646660568676&jsonp=jsonp&mode=2&plat=1";
        private const string replyBaseQuery = @"callback=jQuery3310583964485019705_1646660568676&jsonp=jsonp&ps=20";

        private readonly ILogger logger;
        private string logPrefix = "";
        private readonly Request request;
        private readonly IBiliDynamicRunRecordRepository runRecordRepo;
        private readonly IBiliUserRepository userRepo;
        private readonly IBiliReplyRepository replyRepo;
        private readonly WebBiliUserRepository webUserRepo;

        public WebBiliReplyRepository(ILogger<WebBiliReplyRepository> logger, IHost host)
        {
            this.logger = logger;

            request = host.Services.GetRequiredService<Request>();
            runRecordRepo = host.Services.GetRequiredService<IBiliDynamicRunRecordRepository>();
            userRepo = host.Services.GetRequiredService<IBiliUserRepository>();
            replyRepo = host.Services.GetRequiredService<IBiliReplyRepository>();
            webUserRepo = host.Services.GetRequiredService<WebBiliUserRepository>();
        }

        public async IAsyncEnumerable<RawBiliReply> FromDynamicAsync(BiliDynamic biliDynamic, [EnumeratorCancellation] CancellationToken stoppingToken, int nextPos = 0, bool force = false, int requestTimeout = 10)
        {
            var up = await userRepo.Collection.Find(f => f.UserId == biliDynamic.UserId).Limit(1).FirstOrDefaultAsync(stoppingToken) ?? await webUserRepo.FromIdAsync(biliDynamic.UserId);
            if (up is null)
            {
                logger.LogError("{logPrefix} User {userId} not found.", logPrefix, biliDynamic.UserId);
                yield break;
            }

            logPrefix = $"[{up.UserName}:{biliDynamic.DynamicId}]";
            var runRecordCursor = runRecordRepo.Collection.Find(x => x.DynamicId == biliDynamic.DynamicId).SortByDescending(x => x.Id).Limit(1).ToList();
            BiliDynamicRunRecord? lastRunRecord;
            if (runRecordCursor.Count > 0)
                lastRunRecord = runRecordCursor[0];
            else
                lastRunRecord = null;

            var query = HttpUtility.ParseQueryString(baseQuery);
            query["type"] = threadMap[biliDynamic.DynamicType].ToString();
            query["oid"] = biliDynamic.ThreadId.ToString();
            query["next"] = nextPos.ToString();

            var uriBuilder = new UriBuilder(threadUri);
            uriBuilder.Query = query.ToString();
            RawBiliThreadDataCursor cursor;
            try
            {
                logger.LogDebug("{logPrefix} Start fetching.", logPrefix);
                var response = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers: headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
                if (response is null)
                {
                    logger.LogError("{logPrefix} Failed to get url: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                    yield break;
                }
                logger.LogDebug("{logPrefix} Fetching successful.", logPrefix);

                cursor = response.Data.Cursor;
            }
            catch (Exception e)
            {
                logger.LogError("{logPrefix} Failed to parse url result: {url}\n{ex}", logPrefix, uriBuilder.Uri.AbsoluteUri, e);
                yield break;
            }

            if (!force
                && lastRunRecord != null
                && lastRunRecord.EndTime != null
                && cursor.Count == lastRunRecord.Total)
            {
                logger.LogDebug("{logPrefix} Skip beacuse no new comments.", logPrefix);
                yield break;
            }

            logger.LogDebug("{logPrefix} Start running.", logPrefix);

            BiliDynamicRunRecord runRecord;
            if (lastRunRecord != null && lastRunRecord.EndTime is null)
                runRecord = lastRunRecord;
            else
            {
                runRecord = new BiliDynamicRunRecord()
                {
                    DynamicId = biliDynamic.DynamicId,
                    View = biliDynamic.View,
                    Like = biliDynamic.Like,
                    Total = cursor.Count,
                    StartTime = DateTime.UtcNow
                };
                await runRecordRepo.InsertAsync(runRecord);
            }

            if (runRecord.Progress == -1)
                nextPos = runRecord.Total;
            else
                nextPos = runRecord.Progress;

            while(!cursor.IsEnd)
            {
                logger.LogDebug("{logPrefix} Querying next {next}.", logPrefix, nextPos);
                runRecord.Progress = nextPos;
                query["next"] = nextPos.ToString();
                uriBuilder.Query = query.ToString();
                RawBiliThread? response;
                try
                {
                    response = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers: headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
                    logger.LogDebug("{logPrefix} Queryed next {next} with {replyCount} replies.", logPrefix, nextPos, response?.Data.Replies.Count);
                    if (response is null)
                    {
                        logger.LogError("{logPrefix} Failed to get url: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                        yield break;
                    }

                    cursor = response.Data.Cursor;

                    if (response.Code != 0)
                    {
                        logger.LogError("{logPrefix} API return failed with code {code}: {url}", logPrefix, response.Code, uriBuilder.Uri.AbsoluteUri);
                        yield break;
                    }
                    nextPos = cursor.Next;
                }
                catch (Exception e)
                {
                    logger.LogError("{logPrefix} Failed to parse url result: {url}\n{ex}", logPrefix, uriBuilder.Uri.AbsoluteUri, e);
                    yield break;
                }
                if (response.Data.Replies.Count > 0)
                {
                    foreach (var reply in response.Data.Replies)
                        yield return reply;
                }
            }
            runRecord.Progress = 0;
            runRecord.EndTime = DateTime.UtcNow;
            await runRecordRepo.UpdateAsync(runRecord);
            logger.LogDebug("{logPrefix} Done.", logPrefix);
        }

        public async Task<BiliReply> FromRawAsync(RawBiliReply raw, BiliUser up, BiliDynamic dynamic, int requestTimeout = 10)
        {
            var document = new BiliReply()
            {
                ReplyId = raw.ReplyId,
                Floor = raw.Floor,
                Root = raw.Root,
                ThreadId = raw.ThreadId,
                UpId = up.UserId,
                Parent = raw.Parent,
                Dialog = raw.Dialog,
                Time = raw.Time,
                Like = raw.Like,
                UserId = raw.Member.UserId,
                Content = raw.Content.Message,
                Device = raw.Content.Device,
                Plat = raw.Content.Plat,
                HasFolded = raw.Folder.HasFolded,
                IsFolded = raw.Folder.IsFolded,
                Invisible = raw.Invisible,
                RepliesCount = raw.ReplyCount
            };

            document.User = new BiliReplyUser()
            {
                UserId = raw.Member.UserId,
                UserName = raw.Member.Username,
                Avatar = raw.Member.Avatar,
                Sex = raw.Member.ParsedSex,
                Sign = raw.Member.Sign,
                Level = raw.Member.LevelInfo.Level,
                Vip = raw.Member.VIP.Type,
                Sailings = new List<BiliUserSailing>(),
                Pendants = new List<string> ()
            }; 

            if (raw.Member.UserSailing is RawBiliReplyMemberUserSailing sailing
                && sailing.CardBG is RawBiliReplyMemberUserSailingCardBG cardBG)
            {
                document.User.Sailings.Add(new BiliUserSailing()
                {
                    IsFan = cardBG.Fan?.IsFan,
                    Name = cardBG.Name,
                    Number = cardBG.Fan?.Number
                });
            }

            if (!string.IsNullOrEmpty(raw.Member.Pendant.Name))
                document.User.Pendants.Add(raw.Member.Pendant.Name);

            document.Up = new BiliReplyMiniUser()
            {
                UserId = up.UserId,
                UserName = up.UserName
            };

            if (raw.ReplyCount > 0)
            {
                if (raw.Replies.Count < document.RepliesCount)
                {
                    foreach (var page in Enumerable.Range(1, (int)Math.Ceiling((double)(raw.ReplyCount / 20)) + 1))
                        await FromReplyIdAsync(document.ReplyId, up, dynamic, page, requestTimeout: requestTimeout);
                }
                else
                {
                    foreach(var reply in raw.Replies)
                    {
                        await FromRawAsync(reply, up, dynamic, requestTimeout: requestTimeout);
                    }
                }
            }

            if (await replyRepo.Collection.Find(f => f.ReplyId == document.ReplyId).Limit(1).FirstOrDefaultAsync() is BiliReply biliReply)
            {
                biliReply.Like = document.Like;
                biliReply.RepliesCount = document.RepliesCount;
                biliReply.Plat = document.Plat;
                await replyRepo.UpdateAsync(biliReply);
                logger.LogTrace("{logPrefix} Update reply {replyId}.", logPrefix, document.ReplyId);
                return biliReply;
            }
            else
            {
                await replyRepo.InsertAsync(document);
                logger.LogTrace("{logPrefix} Insert reply {replyId}.", logPrefix, document.ReplyId);
                return document;
            }
        }

        private async Task FromReplyIdAsync(long replyId, BiliUser up, BiliDynamic dynamic, int page, int requestTimeout = 10)
        {
            var query = HttpUtility.ParseQueryString(replyBaseQuery);
            query["pn"] = page.ToString();
            query["root"] = replyId.ToString();
            query["type"] = threadMap[dynamic.DynamicType].ToString();
            query["oid"] = dynamic.ThreadId.ToString();

            var uriBuilder = new UriBuilder(replyUri);
            uriBuilder.Query = query.ToString();

            var response = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
            if (response == null)
                return;

            if (response.Code != 0)
            {
                logger.LogError("{logPrefix} Failed to walk sub replies: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                return;
            }

            if (response.Data.Replies != null && response.Data.Replies.Count > 0)
            {
                foreach(var reply in response.Data.Replies)
                    await FromRawAsync(reply, up, dynamic, requestTimeout: requestTimeout);
            }

        }

        private string ExtractJQJson(Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var buf = new byte[memoryStream.Length];
            memoryStream.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf[40..^1]);
        }

    }
}
