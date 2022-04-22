using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mzr.Share.Models.Bilibili
{
    public class BiliDynamic : BiliBase
    {
        [BsonElement("dynamic_type")]
        public int DynamicType { get; set; }

        [BsonElement("dynamic_id")]
        public long DynamicId { get; set; }

        [BsonElement("orig_dy_id")]
        public long? OriginalDynamicId { get; set; }

        [BsonElement("orig_type")]
        public int? OriginalType { get; set; }

        [BsonElement("video_id")]
        public string? VideoId { get; set; }

        [BsonElement("user_id")]
        public long UserId { get; set; }

        [BsonElement("report_id")]
        public long ReportId { get; set; }

        [BsonElement("thread_id")]
        public long ThreadId { get; set; }

        [BsonElement("target_url")]
        public string? TargetUrl { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("time")]
        public DateTime Time { get; set; }

        [BsonElement("view")]
        public int View { get; set; }

        [BsonElement("like")]
        public int Like { get; set; }
    }
}
