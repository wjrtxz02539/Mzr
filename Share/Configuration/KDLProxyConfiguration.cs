using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Mzr.Share.Configuration
{
    public class KDLProxyConfiguration
    {

        public string Url { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? ApiKey { get; set; }
        public int BatchSize { get; set; } = 0;

        public int ProxyDeleteCount { get; set; } = 3;

        public KDLProxyConfiguration()
        {
        }

        public void Check()
        {
            if (string.IsNullOrEmpty(Url))
                throw new ArgumentException("KDL ProxyPool Configuration missing.");
        }
    }
}
