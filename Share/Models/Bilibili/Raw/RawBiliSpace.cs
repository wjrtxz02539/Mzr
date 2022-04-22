using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliSpace : RawBiliBase
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public RawBiliSpaceData Data { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("msg")]
        public string Msg { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class RawBiliSpaceData
    {
        [JsonPropertyName("cards")]
        public List<RawBiliDynamicDataCard> Cards { get; set; } = null!;

        [JsonPropertyName("has_more")]
        public int HasMore { get; set; }

        [JsonPropertyName("next_offset")]
        public long NextOffset { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
