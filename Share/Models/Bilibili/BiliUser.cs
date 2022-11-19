using MongoDB.Bson.Serialization.Attributes;


namespace Mzr.Share.Models.Bilibili
{

    public class BiliUserSailing
    {
        [BsonElement("name")]
        public string? Name { get; set; }
        [BsonElement("is_fan")]
        public bool? IsFan { get; set; }
        [BsonElement("number")]
        public int? Number { get; set; }
    }
    public class BiliUser : BiliBase
    {
        [BsonElement("user_id")]
        public long UserId { get; set; }
        [BsonElement("username")]
        public string Username { get; set; } = null!;
        [BsonElement("avatar")]
        public string? Avatar { get; set; }
        [BsonElement("sex")]
        public int? Sex { get; set; }
        [BsonElement("sign")]
        public string? Sign { get; set; }
        [BsonElement("level")]
        public int? Level { get; set; }
        [BsonElement("vip")]
        public int? Vip { get; set; }
        [BsonElement("sailings")]
        public List<BiliUserSailing> Sailings { get; set; } = new();
        [BsonElement("pendants")]
        public List<string> Pendants { get; set; } = new();

        [BsonElement("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;

        [BsonElement("usernames")]
        public List<string> Usernames { get; set; } = new();

        [BsonElement("signs")]
        public List<string> Signs { get; set; } = new();

        [BsonElement("ip_list")]
        public List<string> IPList { get; set; } = new();


        [BsonIgnore]
        public string SexString
        {
            get
            {
                return Sex switch
                {
                    0 => "女",
                    1 => "男",
                    _ => "未知",
                };
            }
        }

        [BsonIgnore]
        public string VipString
        {
            get
            {
                return Vip switch
                {
                    1 => "大会员",
                    2 => "年度大会员",
                    _ => "",
                };
            }
        }
    }
}
