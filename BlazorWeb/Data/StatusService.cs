namespace BlazorWeb.Data
{
    using BlazorWeb.Models.Chart;
    using MudBlazor;
    using Mzr.Share.Models.Bilibili;
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
    }
}
