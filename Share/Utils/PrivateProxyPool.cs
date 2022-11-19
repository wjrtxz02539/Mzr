using Microsoft.Extensions.Logging;
using Mzr.Share.Configuration;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.ProxyPool;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;

namespace Mzr.Share.Utils
{
    public class PrivateProxyPool : IProxyPool
    {
        private const string GetMyIPUrl = "https://dev.kdlapi.com/api/getmyip";
        private const string SetWhiteIPUrl = "https://dev.kdlapi.com/api/setipwhitelist";

        private readonly KDLProxyConfiguration configuration;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<string, Proxy> proxyPools = new();
        private readonly SemaphoreSlim poolLock = new(1);
        private readonly Task refreshTask;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private int totalProxyCount = 0;
        private int deleteProxyCount = 0;
        public PrivateProxyPool(MzrConfiguration configuration, ILogger<ProxyPool> logger)
        {
            this.logger = logger;
            this.configuration = configuration.KDLProxy;

            UpdateWhiteIP();

            refreshTask = new(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(10000);
                    if (await GetProxyCount() < this.configuration.BatchSize)
                        await GetMoreProxy();
                }
            });
            refreshTask.Start();
        }

        public void UpdateWhiteIP()
        {
            using var client = new HttpClient();
            var response = client.GetFromJsonAsync<GetMyIPResponse>($"{GetMyIPUrl}?&orderid={configuration.OrderId}&signature={configuration.ApiKey}").Result;
            if (response == null)
                throw new Exception("Failed to get IP.");

            if (response.Code != 0)
                throw new Exception($"Failed to get IP: {response.Message}");

            var setResponse = client.GetFromJsonAsync<SetWhiteIPResponse>($"{SetWhiteIPUrl}?&orderid={configuration.OrderId}&signature={configuration.ApiKey}&iplist={response.Data.IP}").Result;
            if (setResponse == null)
                throw new Exception("Failed to set white list IP.");
            if (setResponse.Code != 0)
                throw new Exception($"Failed to set white list IP: {setResponse.Message}");
        }

        public async Task GetMoreProxy()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<GetDpsResponse>($"{configuration.Url}?&orderid={configuration.OrderId}&signature={configuration.ApiKey}&num={configuration.BatchSize}", cancellationToken: cancellationTokenSource.Token);
                if (response == null)
                {
                    logger.LogError("Failed to get more proxy.");
                    return;
                }

                if (response.Code != 0)
                {
                    logger.LogError("[{code}] Get more proxy failed: {message}.", response.Code, response.Message);
                    return;
                }

                var addTime = DateTime.UtcNow;
                await poolLock.WaitAsync(cancellationTokenSource.Token);
                try
                {
                    foreach (var proxy in response.Data.Proxys)
                    {
                        proxyPools.TryAdd(proxy, new Proxy()
                        {
                            Key = proxy,
                            Url = proxy,
                            AddTime = addTime,
                            IsHttps = true,
                            DeleteCount = 0,
                        });
                    }
                    Interlocked.Add(ref totalProxyCount, response.Data.Proxys.Count);
                    logger.LogDebug("Add {count} proxy.", response.Data.Proxys.Count);
                }
                finally
                {
                    poolLock.Release();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to get more proxy. {ex}", ex);
            }
        }
        public async Task DeleteProxy(Proxy proxy)
        {
            await poolLock.WaitAsync(cancellationTokenSource.Token);
            try
            {
                if (!proxyPools.ContainsKey(proxy.Key))
                    return;
                proxyPools[proxy.Key].DeleteCount++;
                if (proxyPools[proxy.Key].DeleteCount >= configuration.ProxyDeleteCount)
                {
                    proxyPools.TryRemove(new KeyValuePair<string, Proxy>(proxy.Key, proxy));
                    Interlocked.Increment(ref deleteProxyCount);
                    logger.LogDebug("Delete proxy: {proxy}.", proxy);
                }
            }
            finally
            {
                poolLock.Release();
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            refreshTask.Dispose();
        }

        public async Task<Proxy?> GetProxy(int tryCount = 60)
        {
            var tries = 0;
            while (tries < tryCount)
            {
                await poolLock.WaitAsync(cancellationTokenSource.Token);
                try
                {
                    if (proxyPools.IsEmpty)
                    {
                        tries++;
                        logger.LogDebug("[{tries}/{tryCount}] Try to get proxy.", tries, tryCount);
                        poolLock.Release();
                        await Task.Delay(3000);
                        continue;
                    }

                    var random = new Random();
                    var index = random.Next(0, proxyPools.Count);
                    var proxy = proxyPools[proxyPools.Keys.ToList()[index]];
                    poolLock.Release();
                    logger.LogTrace("Get proxy: {proxy}.", proxy.Url);
                    return proxy;
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to get proxy: {ex}", ex);
                    poolLock.Release();
                    await Task.Delay(3000);
                }
            }
            logger.LogDebug("Could not get proxy.");
            return null;
        }

        public async Task<int> GetProxyCount(int tryCount = 3)
        {
            await Task.Yield();
            await poolLock.WaitAsync(cancellationTokenSource.Token);
            try
            {
                return proxyPools.Count;
            }
            finally
            {
                poolLock.Release();
            }
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
