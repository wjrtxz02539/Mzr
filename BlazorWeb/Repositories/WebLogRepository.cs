using BlazorWeb.Models.Web;
using MongoDB.Driver;
using Mzr.Share.Repositories;

namespace BlazorWeb.Repositories
{
    public class WebLogRepository : MongoDBRepository<WebLog>
    {
        public WebLogRepository(IMongoDatabase mongoDatabase, ILogger<WebLogRepository> logger) : base(
            mongoDatabase,
            logger,
            "web_log",
            new List<CreateIndexModel<WebLog>>()
        {
            new CreateIndexModel<WebLog>(Builders<WebLog>.IndexKeys.Ascending(f => f.Username), new CreateIndexOptions() {Background = true}),
            new CreateIndexModel<WebLog>(Builders<WebLog>.IndexKeys.Ascending(f => f.Function), new CreateIndexOptions() {Background = true}),
            new CreateIndexModel<WebLog>(Builders<WebLog>.IndexKeys.Ascending(f => f.Time), new CreateIndexOptions() {Background = true}),
            new CreateIndexModel<WebLog>(Builders<WebLog>.IndexKeys.Ascending(f => f.Status), new CreateIndexOptions() {Background = true}),
        })
        {
        }
    }
}
