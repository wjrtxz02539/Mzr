using System.Text.Json.Serialization;

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
