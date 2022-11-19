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
using System.Diagnostics.SymbolStore;
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
        private readonly IBiliDynamicRepository dynamicRepo;
        private readonly IBiliUserRepository userRepo;
        private readonly IBiliReplyRepository replyRepo;
        private readonly WebBiliUserRepository webUserRepo;

        public WebBiliReplyRepository(ILogger<WebBiliReplyRepository> logger, IHost host)
        {
            this.logger = logger;

            request = host.Services.GetRequiredService<Request>();
            runRecordRepo = host.Services.GetRequiredService<IBiliDynamicRunRecordRepository>();
            dynamicRepo = host.Services.GetRequiredService<IBiliDynamicRepository>();
            userRepo = host.Services.GetRequiredService<IBiliUserRepository>();
            replyRepo = host.Services.GetRequiredService<IBiliReplyRepository>();
            webUserRepo = host.Services.GetRequiredService<WebBiliUserRepository>();
        }

        public async IAsyncEnumerable<RawBiliReply> FromDynamicAsync(BiliDynamic biliDynamic, [EnumeratorCancellation] CancellationToken stoppingToken, int nextPos = 0, bool force = false, int requestTimeout = 10, int retryCount = 100)
        {
            var up = await userRepo.Collection.Find(f => f.UserId == biliDynamic.UserId).Limit(1).FirstOrDefaultAsync(stoppingToken);
            if (up is null)
            {
                up = await webUserRepo.FromIdAsync(biliDynamic.UserId);
                if (up != null)
                {
                    await userRepo.InsertAsync(up);
                }
                else
                {
                    logger.LogError("{logPrefix} User {userId} not found.", logPrefix, biliDynamic.UserId);
                    yield break;
                }
            }

            var dynamic = await dynamicRepo.Collection.Find(f => f.DynamicId == biliDynamic.DynamicId).FirstOrDefaultAsync();
            if(dynamic == null)
            {
                logger.LogError("{logPrefix} Dynamic not found in database: {dynamicId}.", logPrefix, biliDynamic.DynamicId);
                yield break;
            }


            logPrefix = $"[{up.Username}:{dynamic.DynamicId}]";
            var runRecordCursor = runRecordRepo.Collection.Find(x => x.DynamicId == dynamic.DynamicId).SortByDescending(x => x.Id).Limit(1).ToList();
            BiliDynamicRunRecord? lastRunRecord;
            if (runRecordCursor.Count > 0)
                lastRunRecord = runRecordCursor[0];
            else
                lastRunRecord = null;

            var query = HttpUtility.ParseQueryString(baseQuery);
            query["type"] = threadMap[dynamic.DynamicType].ToString();
            query["oid"] = dynamic.ThreadId.ToString();
            query["next"] = nextPos.ToString();

            var uriBuilder = new UriBuilder(threadUri);
            uriBuilder.Query = query.ToString();
            RawBiliThreadDataCursor cursor;
            try
            {
                logger.LogDebug("{logPrefix} Start fetching.", logPrefix);

                RawBiliThread? rawBiliThread;
                var rawBiliThreadCount = 0;
                do
                {
                    rawBiliThread = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers: headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
                    if (rawBiliThread is null || rawBiliThreadCount >= retryCount)
                    {
                        logger.LogError("{logPrefix} Failed to get url: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                        yield break;
                    }
                    rawBiliThreadCount++;
                } while (rawBiliThread.Code != 0);

                logger.LogDebug("{logPrefix} Fetching successful.", logPrefix);
                cursor = rawBiliThread.Data.Cursor;
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
                if (dynamic.UpdatedTime == null)
                {
                    dynamic.UpdatedTime = lastRunRecord.StartTime;
                    dynamic.Reply = lastRunRecord.Total;
                    await dynamicRepo.UpdateAsync(dynamic);
                }
                logger.LogInformation("{logPrefix} Skip beacuse no new comments.", logPrefix);
                yield break;
            }
            else
            {
                dynamic.UpdatedTime = DateTime.UtcNow;
                dynamic.Reply = cursor.Count;
                await dynamicRepo.UpdateAsync(dynamic);
            }

            logger.LogDebug("{logPrefix} Start running.", logPrefix);

            BiliDynamicRunRecord runRecord;
            if (lastRunRecord != null && lastRunRecord.EndTime is null)
                runRecord = lastRunRecord;
            else
            {
                runRecord = new BiliDynamicRunRecord()
                {
                    DynamicId = dynamic.DynamicId,
                    View = dynamic.View,
                    Like = dynamic.Like,
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
                RawBiliThread? rawBiliThread;
                var rawBiliThreadCount = 0;
                try
                {
                    do
                    {
                        rawBiliThread = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers: headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
                        logger.LogDebug("{logPrefix} Queryed next {next} with {replyCount} replies.", logPrefix, nextPos, rawBiliThread?.Data.Replies?.Count ?? 0);
                        if (rawBiliThread is null || rawBiliThreadCount >= retryCount)
                        {
                            logger.LogError("{logPrefix} Failed to get url: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                            yield break;
                        }
                        rawBiliThreadCount++;
                    } while (rawBiliThread.Code != 0);

                    cursor = rawBiliThread.Data.Cursor;

                    if (rawBiliThread.Code != 0)
                    {
                        logger.LogError("{logPrefix} API return failed with code {code}: {url}", logPrefix, rawBiliThread.Code, uriBuilder.Uri.AbsoluteUri);
                        yield break;
                    }
                    nextPos = cursor.Next;
                }
                catch (Exception e)
                {
                    logger.LogError("{logPrefix} Failed to parse url result: {url}\n{ex}", logPrefix, uriBuilder.Uri.AbsoluteUri, e);
                    yield break;
                }
                if (rawBiliThread.Data.Replies is not null && rawBiliThread.Data.Replies.Count > 0)
                {
                    foreach (var reply in rawBiliThread.Data.Replies)
                        yield return reply;
                }
            }
            runRecord.Progress = 0;
            runRecord.EndTime = DateTime.UtcNow;
            await runRecordRepo.UpdateAsync(runRecord);

            dynamic.Reply = runRecord.Total;
            dynamic.UpdatedTime = runRecord.StartTime;
            await dynamicRepo.UpdateAsync(dynamic);
            logger.LogInformation("{logPrefix} Done.", logPrefix);
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
                HasFolded = raw.Folder?.HasFolded,
                IsFolded = raw.Folder?.IsFolded,
                Invisible = raw.Invisible,
                RepliesCount = raw.ReplyCount,
                IP = raw.Content.IPv6
            };
            
            document.User = new BiliReplyUser()
            {
                UserId = raw.Member.UserId,
                Username = raw.Member.Username,
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

            // Update BiliUser to Database
            var dbUser = await userRepo.Collection.Find(f => f.UserId == document.User.UserId).FirstOrDefaultAsync();
            if (dbUser == null)
            {
                dbUser = await webUserRepo.FromIdAsync(document.User.UserId);
                if(dbUser == null)
                { 
                    dbUser = new BiliUser()
                    {
                        UserId = document.User.UserId,
                        Username = document.User.Username,
                        Avatar = document.User.Avatar,
                        Sex = document.User.Sex,
                        Sign = document.User.Sign,
                        Level = document.User.Level,
                        Vip = document.User.Vip,
                        Sailings = document.User.Sailings,
                        Pendants = document.User.Pendants,
                        UpdateTime = DateTime.UtcNow,
                    };
                    if (!string.IsNullOrEmpty(document.IP))
                        dbUser.IPList.Add(document.IP);
                }
                try
                {
                    await userRepo.InsertAsync(dbUser);
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError.Code == 11000)
                        await userRepo.UpdateAsync(dbUser);
                    else
                        logger.LogError("Insert Bili User error: {ex}.", ex.WriteError.ToString());
                }
            }
            else
            {
                dbUser = MergeUserSailingAndPendent(dbUser, document.User);

                if (dbUser.UpdateTime < DateTime.UtcNow)
                {
                    dbUser.Username = document.User.Username;
                    dbUser.Avatar = document.User.Avatar;
                    dbUser.Sex = document.User.Sex;
                    dbUser.Sign = document.User.Sign;
                    dbUser.Level = document.User.Level;
                    dbUser.Vip = document.User.Vip;
                    dbUser.UpdateTime = DateTime.UtcNow;
                }

                if (!dbUser.Usernames.Contains(document.User.Username))
                    dbUser.Usernames.Add(document.User.Username);
                if(!dbUser.Signs.Contains(document.User.Sign))
                    dbUser.Signs.Add(document.User.Sign);
                if (!string.IsNullOrEmpty(document.IP) && !dbUser.IPList.Contains(document.IP))
                    dbUser.IPList.Add(document.IP);

                await userRepo.UpdateAsync(dbUser);
            }

            document.Up = new BiliReplyMiniUser()
            {
                UserId = up.UserId,
                Username = up.Username
            };

            if (raw.ReplyCount > 0)
            {
                if ((raw.Replies?.Count ?? 0) < document.RepliesCount)
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
                biliReply.Time = document.Time;
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

        private async Task FromReplyIdAsync(long replyId, BiliUser up, BiliDynamic dynamic, int page, int requestTimeout = 10, int retryCount = 100)
        {
            var query = HttpUtility.ParseQueryString(replyBaseQuery);
            query["pn"] = page.ToString();
            query["root"] = replyId.ToString();
            query["type"] = threadMap[dynamic.DynamicType].ToString();
            query["oid"] = dynamic.ThreadId.ToString();

            var uriBuilder = new UriBuilder(replyUri);
            uriBuilder.Query = query.ToString();

            RawBiliThread? rawBiliThread;
            var rawBiliThreadCount = 0;
            do
            {
                rawBiliThread = await request.GetFromJsonAsync<RawBiliThread>(uriBuilder.Uri.AbsoluteUri, headers, responseFunc: ExtractJQJson, autoHttps: true, timeout: requestTimeout);
                if (rawBiliThread == null || rawBiliThreadCount >= retryCount)
                {
                    logger.LogError("{logPrefix} Failed to walk sub replies: {url}.", logPrefix, uriBuilder.Uri.AbsoluteUri);
                    return;
                }
            } while (rawBiliThread.Code != 0);

            if (rawBiliThread.Data.Replies != null && rawBiliThread.Data.Replies.Count > 0)
            {
                foreach(var reply in rawBiliThread.Data.Replies)
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

        private BiliUser MergeUserSailingAndPendent(BiliUser user, BiliReplyUser replyUser)
        {
            var dbSailingNames = user.Sailings.Select(s => s.Name).ToList();
            if (replyUser.Sailings != null)
                foreach (var item in replyUser.Sailings)
                {
                    if (!dbSailingNames.Contains(item.Name))
                        user.Sailings.Add(item);
                }

            if(replyUser.Pendants != null)
                foreach (var item in replyUser.Pendants)
                {
                    if (!user.Pendants.Contains(item))
                        user.Pendants.Add(item);
                }

            return user;
        }

    }
}
