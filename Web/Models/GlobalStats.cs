using Mzr.Share.Models.Bilibili;
using System.Collections.Concurrent;

namespace Mzr.Web.Models
{
    public class GlobalStats
    {
        public ConcurrentDictionary<long, UserStats> MonitorUsers { get; set; } = new();
        public List<Tuple<BiliUser, BiliDynamic>> LatestDynamics { get; set; } = new();
        public List<Tuple<int, long, BiliUser?>> MostReplyUsers { get; set; } = new();
    }
    public class UserStats
    {
        public string Username { get; set; } = null!;
        public long UserId { get; set; }
        public long DynamicCount { get; set; }
        public long ReplyCount { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
