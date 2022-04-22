using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Mzr.Share.Models.ProxyPool;
using Mzr.Share.Configuration;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;
using Mzr.Share.Interfaces;
using System.Collections.Concurrent;

namespace Mzr.Share.Utils
{
    public class ProxyPool : IProxyPool
    {
        private KDLProxyConfiguration configuration;
        private HttpClient client;
        private Uri baseUri;
        private Uri getUri;
        private Uri countUri;
        private Uri deleteUri;
        private bool disposedValue;
        private ILogger<ProxyPool> logger;
        private Dictionary<string, int> waitToDelete = new();
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
