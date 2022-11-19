using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Mzr.Share.Configuration;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.ProxyPool;
using System.Net.Http.Json;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;

namespace Mzr.Share.Utils
{
    public class SelfProxyPool : IProxyPool
    {
        private readonly string deleteUrl;
        private readonly string getUrl;
        private readonly string statusUrl;

        private static int deleteCount = 0;
        private static int totalCount = 0;

        private readonly ILogger logger;
        private readonly SelfProxyConfiguration configuration;

        public SelfProxyPool(ILogger<SelfProxyPool> logger, MzrConfiguration configuration)
        {
            this.configuration = configuration.SelfProxy;
            this.logger = logger;

            var queryDict = new Dictionary<string, string>()
            {
                {"latency", this.configuration.Latency.ToString() },
                {"successCount", this.configuration.SuccessCount.ToString() },
                {"failCount", this.configuration.FailureCount.ToString() },
                {"onlyHttps", this.configuration.OnlyHttps.ToString() }
            };

            var baseUri = new Uri(this.configuration.Url);
            getUrl = QueryHelpers.AddQueryString(new Uri(baseUri, "Api/Proxy/Get").AbsoluteUri, queryDict);
            statusUrl = QueryHelpers.AddQueryString(new Uri(baseUri, "Api/Proxy/Status").AbsoluteUri, queryDict);
            deleteUrl = new Uri(baseUri, "Api/Proxy/Delete").AbsoluteUri;

        }
        public async Task DeleteProxy(Proxy proxy)
        {
            using var client = new HttpClient();
            try
            {
                var response = await client.DeleteAsync($"{deleteUrl}{proxy.Key}");
                if (response == null || !response.IsSuccessStatusCode)
                {
                    logger.LogDebug("Failed to delete proxy: {proxy}.", proxy.Url);
                    return;
                }
                Interlocked.Increment(ref deleteCount);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete proxy: {proxy} with : {ex}", proxy.Url, ex);
            }
            await Task.Yield();
        }

        public void Dispose()
        {
        }

        public int GetDeleteProxyCount()
        {
            return deleteCount;
        }

        public async Task<Proxy?> GetProxy(int tryCount = 3)
        {
            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(getUrl);
                if (response == null || !response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to get proxy.");
                    return null;
                }

                var proxy = await response.Content.ReadFromJsonAsync<ProxyResponse>();
                if (proxy == null)
                    throw new Exception("proxy is null.");

                Interlocked.Increment(ref totalCount);
                return new()
                {
                    Url = proxy.Url,
                    Key = proxy.Id.ToString(),
                    AddTime = proxy.AddTime,
                    IsHttps = proxy.Https.Status
                };
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to parse proxy json: {ex}", ex);
                return null;
            }
        }

        public async Task<int> GetProxyCount(int tryCount = 3)
        {
            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(statusUrl);
                if (response == null || !response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to get proxy status.");
                    return 0;
                }

                var status = await response.Content.ReadFromJsonAsync<ProxyStatusResponse>();
                if (status == null)
                {
                    logger.LogError("Failed to parse proxy status.");
                    return 0;
                }

                return (int)status.TotalCount;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to get proxy status: {ex}", ex);
                return 0;
            }
        }

        public int GetTotalProxyCount()
        {
            return totalCount;
        }
    }
}
