using Mzr.Share.Models.Bilibili;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Service.Crawler.Utils
{
    public class StatusDetail
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int DynamicFound = 0;
        public int DynamicRunning = 0;
        public int DynamicProcessed = 0;

        public int ReplyFound = 0;
        public int ReplyRunning = 0;
        public int ReplyProcessed = 0;
        
        public ConcurrentDictionary<long, BiliDynamic> RunningDynamic = new();
    }
}
