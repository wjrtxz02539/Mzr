using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.ProxyPool
{
    public class GetMyIPResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public GetMyIPResponseData Data { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class GetMyIPResponseData
    {
        [JsonPropertyName("ip")]
        public string IP { get; set; } = null!;
    }
}
