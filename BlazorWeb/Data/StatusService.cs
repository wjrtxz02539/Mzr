namespace BlazorWeb.Data
{
    using BlazorWeb.Models.Chart;
    using BlazorWeb.Models.Configurations;
    using BlazorWeb.Models.Web;
    using MongoDB.Bson;
    using MudBlazor;
    using Mzr.Share.Models.Bilibili;
    using System.Collections.Concurrent;

    public class StatusService
    {
        // Reply increase daily
        public TimeLineDefinition? DailyReplyLineChart = null;

        // Reply increase daily per up
        public TimeLineDefinition? DailyReplyLineChartByUp = null;
        public Tuple<List<BiliUser>, List<double>> DailyReplyTotalByUp = new(new(), new());
        public Tuple<List<BiliUser>, List<double>> MonthlyReplyTotalByUp = new(new(), new());
        public TimeLineDefinition? MonthlyReplyLineChart = null;

        // Running Dynamic
        public List<BiliDynamic> RunningDynamics = new();

        public Dictionary<long, List<BiliDynamic>> RunningDynamicsPerUp = new();

        public Dictionary<long, BiliUser> MonitoredUp = new();

        // Today's Dynamic
        public List<BiliDynamic> TodayDynamics = new();

        public long TodayReplyCount;
        public long TotalReplyCount;

        // Top user daily
        public List<Tuple<int, long, BiliUser?>> TopUsers = new();

        // For FileExportWorker
        public ConcurrentQueue<WebFile> ExportWaitingQueue { get; } = new();
        public ConcurrentDictionary<ObjectId, WebFile> ExportRunningDict { get; } = new();
        public WebConfiguration configuration { get; }

        private ILogger logger;
        public StatusService(ILogger<StatusService> logger, WebConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public bool SubmitExportJob(WebFile file)
        {
            if (ExportWaitingQueue.Count > configuration.FileExportQueueLength)
            {
                logger.LogError("Export queue full, WebFile id: {id}.", file.Id);
                return false;
            }

            ExportWaitingQueue.Enqueue(file);
            return true;
        }
    }
}
