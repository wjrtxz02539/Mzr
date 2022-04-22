using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.ProxyPool
{
    public class GetResponse
    {
        [JsonPropertyName("anonymous")]
        public string? Anonymous { get; set; }

        [JsonPropertyName("check_count")]
        public int CheckCount { get; set; }

        [JsonPropertyName("fail_count")]
        public int FailCount { get; set; }

        [JsonPropertyName("https")]
        public bool IsHttps { get; set; }

        [JsonPropertyName("last_status")]
        public bool LastStatus { get; set; }

        [JsonPropertyName("last_time")]
        public string? LastTime { get; set; }

        [JsonPropertyName("proxy")]
        public string Proxy { get; set; } = null!;

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

    }
}
