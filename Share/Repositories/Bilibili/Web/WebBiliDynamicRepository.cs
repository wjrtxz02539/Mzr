using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using Mzr.Share.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili.Web
{
    public class WebBiliDynamicRepository
    {
        private readonly Request request;
        private readonly Uri spaceUri = new Uri(@"http://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history");
        private static Dictionary<string, string> headers = new() { { "referer", "https://t.bilibili.com/" } };
        private readonly IRawBiliDynamicRepository rawDynamicRepo;
        private readonly ILogger logger;
        public WebBiliDynamicRepository(IHost host)
        {
            request = host.Services.GetRequiredService<Request>();
            rawDynamicRepo = host.Services.GetRequiredService<IRawBiliDynamicRepository>();
            logger = host.Services.GetRequiredService<ILogger<WebBiliDynamicRepository>>();
        }

        public async IAsyncEnumerable<BiliDynamic> FromUserIdAsync(long userId, long offsetDynamicId = 0, int skip = 0,
            int? maxDepth = null, int? maxDays = null, bool force = false, int requestTimeout = 10, int retryCount = 100)
        {
            var logPrefix = $"[Space][{userId}]";
            var url = string.Format(@"{0}?host_uid={1}&offset_dynamic_id={2}&need_top=1&platform=web", spaceUri.AbsoluteUri, userId, offsetDynamicId);

            RawBiliSpace? rawSpace;
            var spaceCount = 0;
            do
            {
                rawSpace = await request.GetFromJsonAsync<RawBiliSpace>(url, timeout: requestTimeout, autoHttps: true, headers: headers);
                if (rawSpace == null || spaceCount >= retryCount)
                {
                    logger.LogError("{logPrefix} Failed to get url {url}.", url);
                    yield break;
                }
                spaceCount++;
            } while (rawSpace.Code != 0);

            if (rawSpace == null || rawSpace.Data == null)
            {
                logger.LogError("{logPrefix} Failed to get space detail's data.\n{response}", logPrefix, rawSpace);
                yield break;
            }

            if (rawSpace.Data.Cards != null)
            {
                List<BiliDynamic> dynamicList = new();

                foreach (var card in rawSpace.Data.Cards)
                {
                    if (card == null)
                        continue;

                    var document = await rawDynamicRepo.FromCardDocument(card);
                    if (document == null)
                        continue;
                    dynamicList.Add(document);
                    logger.LogInformation("{logPrefix} Found dynamic {dynamicId} at {time}.", logPrefix, document.DynamicId, document.Time);
                }

                DateTime? OldestTime = null;
                if (maxDays.HasValue)
                    OldestTime = DateTime.UtcNow.AddDays(-maxDays.Value);

                dynamicList.Sort((x, y) => -x.Time.CompareTo(y.Time));
                foreach (var dynamic in dynamicList)
                {
                    if (skip > 0)
                    {
                        skip--;
                        continue;
                    }

                    if (maxDepth.HasValue)
                    {
                        if (maxDepth >= 0)
                            maxDepth--;
                        else
                        {
                            logger.LogInformation("{logPrefix} Hit depth limit.", logPrefix);
                            yield break;
                        }
                    }

                    if (OldestTime.HasValue)
                    {
                        if (dynamic.Time < OldestTime)
                        {
                            logger.LogInformation("{logPrefix} Hit time limit.", logPrefix);
                            yield break;
                        }
                    }

                    yield return dynamic;
                }

                if (rawSpace.Data.HasMore == 1)
                {
                    await foreach (var dynamic in FromUserIdAsync(userId: userId, offsetDynamicId: rawSpace.Data.NextOffset, skip: skip, maxDepth: maxDepth, maxDays: maxDays, force: force, requestTimeout: requestTimeout))
                        yield return dynamic;
                }
            }
            else
            {
                if (rawSpace.Data.HasMore == 0)
                    logger.LogInformation("{logPrefix} Has not more dynamic.", logPrefix);
                else
                    logger.LogError("{logPrefix} Failed to get space detail.\n{response}", logPrefix, rawSpace);
            }

        }
    }
}
