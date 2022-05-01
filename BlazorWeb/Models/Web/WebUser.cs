using MongoDB.Bson.Serialization.Attributes;
using Mzr.Share.Models;

namespace BlazorWeb.Models.Web
{
    public class WebUser : MongoDBBase
    {
        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [BsonElement("login_time")]
        public DateTime? LoginTime { get; set; } = null;

        [BsonElement("register_time")]
        public DateTime? RegisterTime { get; set; } = null;

        [BsonElement("visit_time")]
        public DateTime? VisitTime { get; set; } = null;

        [BsonElement("visit_count")]
        public int VisitCount { get; set; }

        [BsonElement("banned")]
        public bool Banned { get; set; } = false;

        [BsonElement("query_count")]
        public int QueryCount { get; set; } = 0;
    }
}
