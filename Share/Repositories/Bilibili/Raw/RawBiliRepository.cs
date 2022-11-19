using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili.Raw;

namespace Mzr.Share.Repositories.Bilibili.Raw
{
    public class RawBiliRepository<T> : IRawBiliRepository<T> where T : RawBiliBase
    {
        public readonly ILogger Logger;
        public readonly IHost Host;

        public RawBiliRepository(ILogger logger, IHost host)
        {
            Host = host;
            Logger = logger;
        }

        public void Dispose()
        {
        }
    }
}
