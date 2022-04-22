using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.ProxyPool
{
    public class CountStatus
    {
        [JsonPropertyName("https")]
        public int Https { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
    public class CountResponse
    {
        [JsonPropertyName("count")]
        public CountStatus? Status { get; set; }
    }
}
