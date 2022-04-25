using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliDynamicData
    {
        [JsonPropertyName("card")]
        public RawBiliDynamicDataCard Card { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class RawBiliDynamicDataCard
    {
        [JsonPropertyName("card")]
        public string Card { get; set; } = null!;

        [JsonPropertyName("desc")]
        public RawBiliDynamicDataCardDesc Desc { get; set; } = null!;

        [JsonExtensionData]
        public IDictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class RawBiliDynamicDataCardDesc
    {
        [JsonPropertyName("comment")]
        public int Comment { get; set; }

        [JsonPropertyName("dynamic_id_str")]
        public string DynamicIdStr { get; set; } = null!;

        [JsonPropertyName("like")]
        public int Like { get; set; }

        [JsonPropertyName("view")]
        public int View { get; set; }

        [JsonPropertyName("orig_by_id_str")]
        public string? OriginalByIdStr { get; set; }

        [JsonPropertyName("orig_type")]
        public int OriginalType { get; set; }

        [JsonPropertyName("rid_str")]
        public string? RidStr { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JsonElement>? ExtensionData { get; set; }

        [JsonIgnore]
        public long DynamicId { get { return Convert.ToInt64(DynamicIdStr); } }

        [JsonIgnore]
        public DateTime Date { get { return DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime; } }

        [JsonIgnore]
        public long OriginalById { get { return Convert.ToInt64(OriginalByIdStr); } }

        [JsonIgnore]
        public long Rid { get { return Convert.ToInt64(RidStr); } }
    }
}
