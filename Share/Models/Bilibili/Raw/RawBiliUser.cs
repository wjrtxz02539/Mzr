using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliUser : RawBiliBase
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("ttl")]
        public int TTL { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        [JsonPropertyName("data")]
        public RawBiliUserData Data { get; set; } = null!;
    }
}
