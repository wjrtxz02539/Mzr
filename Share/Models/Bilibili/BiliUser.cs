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
                switch (Sex)
                {
                    case 0:
                        return "女";
                    case 1:
                        return "男";
                    default:
                        return "未知";
                }
            }
        }

        [BsonIgnore]
        public string VipString
        {
            get
            {
                switch (Vip)
                {
                    case 1:
                        return "大会员";
                    case 2:
                        return "年度大会员";
                    default:
                        return "";
                }
            }
        }
    }
}
