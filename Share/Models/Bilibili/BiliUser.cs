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
        public string? UserName { get; set; }
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
        public List<BiliUserSailing>? Sailings { get; set; }
        [BsonElement("pendants")]
        public List<string>? Pendants { get; set; }
    }
}
