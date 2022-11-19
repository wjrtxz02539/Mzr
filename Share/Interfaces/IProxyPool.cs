using Mzr.Share.Models.ProxyPool;

namespace Mzr.Share.Interfaces
{
    public interface IProxyPool : IDisposable
    {
        public Task<Proxy?> GetProxy(int tryCount = 3);

        public Task<int> GetProxyCount(int tryCount = 3);

        public Task DeleteProxy(Proxy proxy);

        public int GetTotalProxyCount();

        public int GetDeleteProxyCount();
    }
}
