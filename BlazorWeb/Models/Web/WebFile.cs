using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mzr.Share.Models;

namespace BlazorWeb.Models.Web
{
    public class WebFile : MongoDBBase
    {
        [BsonElement("filename")]
        [BsonRequired]
        public string Filename { get; set; } = null!;

        [BsonElement("username")]
        [BsonRequired]
        public string Username { get; set; } = null!;

        [BsonElement("created_time")]
        [BsonRequired]
        public DateTime CreatedTime { get; set; }

        [BsonElement("expired_time")]
        [BsonRequired]
        public DateTime ExpiredTime { get; set; }

        [BsonElement("function")]
        [BsonRequired]
        public WebFileFunction Function { get; set; }

        [BsonElement("parameters")]
        [BsonRequired]
        public Dictionary<string, object?> Parameters { get; set; } = new();

        [BsonElement("progress")]
        [BsonRequired]
        public double Progress { get; set; } = 0;

        [BsonElement("status")]
        [BsonRequired]
        public WebFileStatusEnum Status { get; set; } = WebFileStatusEnum.Init;

        [BsonElement("error")]
        public string? Error { get; set; } = null;

        [BsonElement("gridfs_id")]
        public ObjectId GridfsId { get; set; }
    }
}
