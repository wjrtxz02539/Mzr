using MongoDB.Bson.Serialization.Attributes;

namespace Mzr.Share.Models.Bilibili
{
    public class BiliReplyUser
    {
        [BsonElement("user_id")]
        public long UserId { get; set; }
        [BsonElement("username")]
        public string? Username { get; set; }
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

    public class BiliReplyMiniUser
    {
        [BsonElement("user_id")]
        public long UserId { get; set; }
        [BsonElement("username")]
        public string? Username { get; set; }
    }
    public class BiliReply : BiliBase
    {
        [BsonElement("up_id")]
        public long UpId { get; set; }
        [BsonElement("reply_id")]
        public long ReplyId { get; set; }
        [BsonElement("thread_id")]
        public long ThreadId { get; set; }
        [BsonElement("floor")]
        public int Floor { get; set; }
        [BsonElement("root")]
        public long? Root { get; set; }
        [BsonElement("parent")]
        public long? Parent { get; set; }
        [BsonElement("dialog")]
        public long? Dialog { get; set; }
        [BsonElement("time")]
        public DateTime Time { get; set; }
        [BsonElement("like")]
        public int? Like { get; set; }
        [BsonElement("user_id")]
        public long UserId { get; set; }
        [BsonElement("user")]
        public BiliReplyUser User { get; set; } = null!;
        [BsonElement("up")]
        public BiliReplyMiniUser? Up { get; set; }
        [BsonElement("content")]
        public string? Content { get; set; }
        [BsonElement("device")]
        public string? Device { get; set; }
        [BsonElement("replies_count")]
        public int? RepliesCount { get; set; }
        [BsonElement("has_folded")]
        public bool? HasFolded { get; set; }
        [BsonElement("is_folded")]
        public bool? IsFolded { get; set; }
        [BsonElement("invisible")]
        public bool? Invisible { get; set; }
        [BsonElement("plat")]
        public int? Plat { get; set; }
        [BsonElement("replies")]
        public List<string>? Replies { get; set; }
    }
}
