using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliUserData
    {
        [JsonPropertyName("mid")]
        public long UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("sex")]
        public string Sex { get; set; } = null!;

        [JsonPropertyName("sign")]
        public string Sign { get; set; } = null!;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("vip")]
        public RawBiliUserDataVip Vip { get; set; } = null!;

        [JsonPropertyName("face")]
        public string Avatar { get; set; } = null!;

        [JsonPropertyName("pendant")]
        public RawBiliUserDataPendant Pendant { get; set; } = null!;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        public int ParsedSex
        {
            get
            {
                return Sex switch
                {
                    "女" => 0,
                    "男" => 1,
                    _ => 2,
                };
            }
        }
    }

    public class RawBiliUserDataVip
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
    }

    public class RawBiliUserDataPendant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
