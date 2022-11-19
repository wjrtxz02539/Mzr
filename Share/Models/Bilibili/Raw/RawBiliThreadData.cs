using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliThreadData
    {
        [JsonPropertyName("cursor")]
        public RawBiliThreadDataCursor Cursor { get; set; } = null!;

        [JsonPropertyName("replies")]
        public List<RawBiliReply>? Replies { get; set; } = new List<RawBiliReply>();
    }

    public class RawBiliThreadDataCursor
    {
        [JsonPropertyName("all_count")]
        public int Count { get; set; }

        [JsonPropertyName("is_begin")]
        public bool IsBegin { get; set; }

        [JsonPropertyName("is_end")]
        public bool IsEnd { get; set; }

        [JsonPropertyName("next")]
        public int Next { get; set; }

        [JsonPropertyName("prev")]
        public int Prev { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

}
