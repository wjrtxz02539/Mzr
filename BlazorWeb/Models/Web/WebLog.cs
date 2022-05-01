using MongoDB.Bson.Serialization.Attributes;
using Mzr.Share.Models;

namespace BlazorWeb.Models.Web
{
    public class WebLog : MongoDBBase
    {
        [BsonElement("function")]
        public string Function { get; set; } = string.Empty;

        [BsonElement("parameters")]
        public Dictionary<string, object?> Parameters { get; set; } = new();

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("time")]
        public DateTime Time { get; set; } 

        [BsonElement("elasped")]
        public long Elasped { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("url")]
        public string Url { get; set; } = string.Empty;

        [BsonElement("error")]
        public string Error { get; set; } = string.Empty;
    }
}
