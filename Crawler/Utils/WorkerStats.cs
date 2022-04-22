using Mzr.Service.Crawler.Worker;
using System.Collections.Concurrent;

namespace Mzr.Service.Crawler.Utils
{
    public class WorkerStats
    {
        public ConcurrentDictionary<long, UserWorker> Workers = new();
    }
}
