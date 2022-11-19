using Microsoft.Extensions.Logging;
using Mzr.Share.Configuration;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.ProxyPool;
using System.Net.Http.Json;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;


namespace Mzr.Share.Utils
{
    public class TunnelProxyPool : IProxyPool
    {
        private const string GetMyIPUrl = "https://dev.kdlapi.com/api/getmyip";
        private const string SetWhiteIPUrl = "https://dev.kdlapi.com/api/setipwhitelist";

        private readonly KDLProxyConfiguration configuration;
        private readonly Proxy proxy;

        public TunnelProxyPool(MzrConfiguration configuration)
        {
            this.configuration = configuration.KDLProxy;
            proxy = new()
            {
                Url = this.configuration.Url,
                Key = this.configuration.Url,
                AddTime = DateTime.UtcNow,
                IsHttps = true
            };

            UpdateWhiteIP();
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
        public async Task DeleteProxy(Proxy proxy)
        {
            await Task.Yield();
        }

        public void Dispose()
        {
            return;
        }

        public int GetDeleteProxyCount()
        {
            return 0;
        }

        public async Task<Proxy?> GetProxy(int tryCount = 3)
        {
            await Task.Yield();
            return proxy;
        }

        public async Task<int> GetProxyCount(int tryCount = 3)
        {
            await Task.Yield();
            return 1;
        }

        public int GetTotalProxyCount()
        {
            return 1;
        }
    }
}
