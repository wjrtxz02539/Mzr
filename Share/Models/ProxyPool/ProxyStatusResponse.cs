using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Models.ProxyPool
{
    public class ProxyStatusResponse
    {
        public long TotalCount { get; set; }
        public long HttpsCount { get; set; }
        public long CheckingCount { get; set; }
        public long WaitToCheckCount { get; set; }
        public long FoundCount { get; set; }
    }
}
