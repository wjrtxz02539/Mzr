using BlazorWeb.Data;
using BlazorWeb.Models.Configurations;
using BlazorWeb.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MudBlazor;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using System.Diagnostics;

namespace BlazorWeb.Workers
{
    public class StatusUpdateWorker : BackgroundService
    {
        private readonly StatusService statusService;
        private readonly WebConfiguration webConfiguration;
        private readonly IBiliDynamicRunRecordRepository recordRepo;
        private readonly IBiliDynamicRepository dynamicRepo;
        private readonly IBiliReplyRepository replyRepo;
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        public StatusUpdateWorker(StatusService statusService, WebConfiguration webConfiguration,
            IBiliDynamicRunRecordRepository recordRepo, IBiliDynamicRepository dynamicRepo, IBiliReplyRepository replyRepo,
            ILogger<StatusUpdateWorker> logger, IServiceProvider serviceProvider)
        {
            this.statusService = statusService;
            this.webConfiguration = webConfiguration;
            this.recordRepo = recordRepo;
            this.dynamicRepo = dynamicRepo;
            this.replyRepo = replyRepo;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var task = new Task(async () => await Refresh(stoppingToken));
            task.Start();

            await task.WaitAsync(stoppingToken);
        }

        private async Task Refresh(CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested)
            {
                var dateRange = DateTimeTools.GetSpecificWholeDayRangeUtc(DateTime.Now);

                using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var replyService = scope.ServiceProvider.GetRequiredService<BiliReplyService>();
                    var userService = scope.ServiceProvider.GetRequiredService<BiliUserService>();
                    var dynamicService = scope.ServiceProvider.GetRequiredService<BiliDynamicService>();

                    stopWatch.Restart();
                    statusService.DailyReplyStatus = new List<ChartSeries>() {
                    new ChartSeries()
                    {
                        Name = "评论数",
                        Data = await replyService.HourlyAggregateAsync(startTime: dateRange.Item1, endTime: dateRange.Item2)
                    }}
                    ;

                    statusService.DailyReplyStatusByUp.Clear();
                    statusService.MonitoredUp.Clear();
                    statusService.DailyReplyTotalByUp.Item1.Clear();
                    statusService.DailyReplyTotalByUp.Item2.Clear();
                    var totalByUp = new List<Tuple<long, BiliUser>>();
                    foreach (var upId in webConfiguration.MonitorUserIds)
                    {
                        var status = await replyService.HourlyAggregateAsync(startTime: dateRange.Item1, endTime: dateRange.Item2, upId: upId);
                        var up = await userService.GetByUserId(upId);

                        var series = new ChartSeries()
                        {
                            Name = up?.Username ?? upId.ToString(),
                            Data = status
                        };

                        statusService.DailyReplyStatusByUp.Add(series);
                        if (up != null)
                        {
                            statusService.MonitoredUp.Add(up.UserId, up);
                            var replyCount = await replyRepo.Collection.CountDocumentsAsync(f => f.Time >= dateRange.Item1 && f.Time <= dateRange.Item2 && f.UpId == up.UserId, cancellationToken: cancellationToken);
                            totalByUp.Add(new(replyCount, up));
                        }
                    }

                    totalByUp.Sort((x, y) => y.Item1.CompareTo(x.Item1));
                    foreach (var t in totalByUp)
                    {
                        statusService.DailyReplyTotalByUp.Item1.Add(t.Item2);
                        statusService.DailyReplyTotalByUp.Item2.Add(t.Item1);
                    }

                    statusService.RunningDynamics.Clear();
                    statusService.RunningDynamicsPerUp.Clear();
                    await recordRepo.Collection.Find(f => f.EndTime == null).SortBy(f => f.Id).ForEachAsync(async r =>
                    {
                        if (await dynamicService.GetByDynamicId(r.DynamicId) is BiliDynamic dynamic)
                        {
                            statusService.RunningDynamics.Add(dynamic);

                            if (statusService.RunningDynamicsPerUp.ContainsKey(dynamic.UserId))
                                statusService.RunningDynamicsPerUp[dynamic.UserId].Add(dynamic);
                            else
                                statusService.RunningDynamicsPerUp[dynamic.UserId] = new() { dynamic };
                        }
                    }, cancellationToken: cancellationToken);

                    statusService.TodayDynamics.Clear();
                    await dynamicRepo.Collection.Find(f => f.Time >= dateRange.Item1 & f.Time <= dateRange.Item2).ForEachAsync(r =>
                    {
                        statusService.TodayDynamics.Add(r);
                    }, cancellationToken: cancellationToken);


                    statusService.TopUsers.Clear();
                    statusService.TopUsers = await replyService.GetTopUsersAsync(startTime: dateRange.Item1, endTime: dateRange.Item2, cancellationToken: cancellationToken);

                    statusService.TodayReplyCount = await replyRepo.Collection.CountDocumentsAsync(f => f.Time >= dateRange.Item1 && f.Time <= dateRange.Item2, cancellationToken: cancellationToken);
                    statusService.TotalReplyCount = await replyRepo.Collection.EstimatedDocumentCountAsync(cancellationToken: cancellationToken);

                    stopWatch.Stop();
                    logger.LogInformation("Update completed, using {ms} ms.", stopWatch.ElapsedMilliseconds);
                }
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }

        }
    }
}
