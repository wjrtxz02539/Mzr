using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliDynamic : RawBiliBase
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("msg")]
        public string Msg { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        [JsonPropertyName("data")]
        public RawBiliDynamicData Data { get; set; } = null!;
    }
}
