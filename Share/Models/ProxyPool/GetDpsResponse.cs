using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.ProxyPool
{
    public class GetDpsResponse
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public GetDpsResponseData Data { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class GetDpsResponseData
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("dedup_count")]
        public int DeDupCount { get; set; }

        [JsonPropertyName("order_left_count")]
        public int OrderLeftCount { get; set; }

        [JsonPropertyName("proxy_list")]
        public List<string> Proxys { get; set; } = new();
    }
}
