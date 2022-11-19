using Microsoft.Extensions.Logging;
using Mzr.Share.Configuration;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.ProxyPool;
using System.Net.Http.Json;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;

namespace Mzr.Share.Utils
{
    public class ProxyPool : IProxyPool
    {
        private readonly KDLProxyConfiguration configuration;
        private readonly HttpClient client;
        private readonly Uri baseUri;
        private readonly Uri getUri;
        private readonly Uri countUri;
        private readonly Uri deleteUri;
        private bool disposedValue;
        private readonly ILogger<ProxyPool> logger;
        private readonly Dictionary<string, int> waitToDelete = new();
        private int deleteProxyCount = 0;
        private int totalProxyCount = 0;

        public ProxyPool(MzrConfiguration configuration, ILogger<ProxyPool> logger)
        {
            this.configuration = configuration.KDLProxy;

            baseUri = new Uri(this.configuration.Url);
            getUri = new Uri(baseUri, "get/");
            countUri = new Uri(baseUri, "count/");
            deleteUri = new Uri(baseUri, "delete/");

            client = new HttpClient();
            this.logger = logger;
            this.logger.LogInformation("ProxyPool Initialized.");
        }

        public async Task<Proxy?> GetProxy(int tryCount = 3)
        {
            var count = 0;
            do
            {
                try
                {
                    var response = await client.GetFromJsonAsync<GetResponse>(getUri);
                    Interlocked.Increment(ref totalProxyCount);
                    if (response != null && !string.IsNullOrEmpty(response.Proxy))
                        return new Proxy()
                        {
                            Url = response.Proxy,
                            Key = response.Proxy,
                            DeleteCount = 0,
                            AddTime = DateTime.UtcNow,
                            IsHttps = response.IsHttps,
                        };
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Failed to GetProxy. {ex}", ex);
                    await Task.Delay(10000);
                }
                finally
                {
                    count++;
                }

            } while (count < tryCount);

            logger.LogCritical("Failed to GetProxy within retry count.");
            return null;
        }

        public async Task<int> GetProxyCount(int tryCount = 3)
        {
            var count = 0;
            do
            {
                try
                {
                    var response = await client.GetFromJsonAsync<CountResponse>(countUri);
                    if (response != null && response.Status != null)
                        return response.Status.Total;
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Failed to GetProxyCount. {ex}", ex);
                }
                finally
                {
                    count++;
                }
            } while (count < tryCount);

            logger.LogCritical("Failed to GetProxyCount within retry count.");
            return 0;
        }

        public async Task DeleteProxy(Proxy proxy)
        {
            if (waitToDelete.ContainsKey(proxy.Key))
            {
                if (waitToDelete[proxy.Key] > 3)
                {
                    await client.GetAsync($"{deleteUri}?proxy={proxy.Key}");
                    waitToDelete.Remove(proxy.Key);
                    Interlocked.Increment(ref deleteProxyCount);
                    logger.LogDebug("Delete proxy {proxy}.", proxy);
                }
                else
                    waitToDelete[proxy.Key]++;
            }
            else
                waitToDelete[proxy.Key] = 1;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int GetTotalProxyCount()
        {
            return totalProxyCount;
        }

        public int GetDeleteProxyCount()
        {
            return deleteProxyCount;
        }
    }
}
