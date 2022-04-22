using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Mzr.Service.Crawler.Utils;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Utils;
using MzrRequest = Mzr.Share.Utils.Request;

namespace Mzr.Service.Crawler.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public readonly WorkerStats Stats;
        public readonly IProxyPool ProxyPool;
        public List<RequestStatus> RequestStatuses = new();
        public Dictionary<long, BiliDynamic> RunningDynamic = new();

        public IndexModel(ILogger<IndexModel> logger, WorkerStats workerStats, IProxyPool proxyPool)
        {
            _logger = logger;
            Stats = workerStats;
            ProxyPool = proxyPool;
        }

        public void OnGet()
        {
            RequestStatuses = MzrRequest.FailedRequests.Values.ToList();
            RequestStatuses.Sort((x, y) => x.LastTime.CompareTo(y.LastTime));

            foreach(var item in Stats.Workers.Values)
            {
                foreach(var pair in item.Status.RunningDynamic)
                    RunningDynamic.Add(pair.Key, pair.Value);
            }
        }
    }
}