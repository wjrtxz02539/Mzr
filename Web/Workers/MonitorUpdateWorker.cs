using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models;
using Mzr.Web.Models.Configurations;
using Mzr.Web.Services;

namespace Mzr.Web.Workers
{
    public class MonitorUpdateWorker : BackgroundService
    {
        private readonly WebConfiguration configuration;
        private readonly GlobalStats globalStats;
        private readonly IBiliUserRepository userRepo;
        private readonly IBiliDynamicRepository dynamicRepo;
        private readonly IBiliReplyRepository replyRepo;
        private readonly IBiliDynamicRunRecordRepository dynamicRunRecordRepo;
        private readonly BiliReplyService replyService;
        public MonitorUpdateWorker(WebConfiguration configuration, GlobalStats globalStats, IBiliUserRepository userRepo,
            IBiliDynamicRepository dynamicRepo, IBiliReplyRepository replyRepo, IBiliDynamicRunRecordRepository dynamicRunRecordRepo,
            BiliReplyService replyService)
        {
            this.configuration = configuration;
            this.globalStats = globalStats;
            this.userRepo = userRepo;
            this.dynamicRepo = dynamicRepo;
            this.replyRepo = replyRepo;
            this.dynamicRunRecordRepo = dynamicRunRecordRepo;
            this.replyService = replyService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            var task = new Task(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
                do
                {
                    await Refresh();
                } while (await timer.WaitForNextTickAsync(stoppingToken));
            });
            task.Start();
            await task.WaitAsync(stoppingToken);
        }

        private async Task Refresh()
        {
            // Refresh Latest Dynamics
            var dynamics = await dynamicRepo.Collection.Find(new BsonDocument()).SortByDescending(f => f.Time).Limit(10).ToListAsync();
            globalStats.LatestDynamics.Clear();
            foreach (var dynamic in dynamics)
            {
                var user = await userRepo.Collection.Find(f => f.UserId == dynamic.UserId).FirstOrDefaultAsync();
                if (user == null)
                    continue;
                globalStats.LatestDynamics.Add(new Tuple<BiliUser, BiliDynamic>(user, dynamic));
            }

            // Refresh MostReplyUsers
            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = DateTime.UtcNow;
            globalStats.MostReplyUsers = await replyService.GetTopUsersAsync(startTime, endTime);

            // Refresh MonitorUserIds
            foreach (var userId in configuration.MonitorUserIds)
            {
                var user = await userRepo.Collection.Find(f => f.UserId == userId).FirstOrDefaultAsync();
                if (user != null)
                {
                    var lastDynamic = await dynamicRepo.Collection.Find(f => f.UserId == userId).SortByDescending(f => f.Time).FirstOrDefaultAsync();
                    var stats = new UserStats()
                    {
                        UserId = userId,
                        Username = user.Username ?? string.Empty,
                        UpdateTime = lastDynamic?.Time.ToUniversalTime()
                    };
                    stats.DynamicCount = await dynamicRepo.Collection.CountDocumentsAsync(f => f.UserId == userId);
                    stats.ReplyCount = await replyRepo.Collection.CountDocumentsAsync(f => f.UpId == userId);

                    globalStats.MonitorUsers[userId] = stats;
                }
            }

            
        }
    }
}
