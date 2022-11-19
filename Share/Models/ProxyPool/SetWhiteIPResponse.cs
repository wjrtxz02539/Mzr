using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.ProxyPool
{
    public class SetWhiteIPResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
