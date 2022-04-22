using MongoDB.Bson.Serialization.Attributes;

namespace Mzr.Share.Models.Bilibili
{
    public class BiliDynamicRunRecord : BiliBase
    {
        [BsonElement("dynamic_id")]
        public long DynamicId { get; set; }

        [BsonElement("total")]
        public int Total { get; set; }

        [BsonElement("view")]
        public int View { get; set; }

        [BsonElement("like")]
        public int Like { get; set; }

        [BsonElement("progress")]
        public int Progress { get; set; } = -1;

        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }

        [BsonElement("end_time")]
        public DateTime? EndTime { get; set; }
    }
}
