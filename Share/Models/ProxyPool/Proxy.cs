using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Models.ProxyPool
{
    public class Proxy
    {
        public string Key { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DeleteCount { get; set; } = 0;
        public DateTime AddTime { get; set; }
        public bool IsHttps { get; set; } = false;
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}
