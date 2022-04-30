using BlazorWeb.Models.Web;
using MongoDB.Driver;
using Mzr.Share.Repositories;

namespace BlazorWeb.Repositories
{
    public class WebUserRepository : MongoDBRepository<WebUser>
    {
        public WebUserRepository(IMongoDatabase mongoDatabase, ILogger<WebUserRepository> logger) : base(
            mongoDatabase,
            logger,
            "web_user",
            new List<CreateIndexModel<WebUser>>()
        {
            new CreateIndexModel<WebUser>(Builders<WebUser>.IndexKeys.Ascending(f => f.Username), new CreateIndexOptions() {Unique = true, Background = true}),
        })
        {
        }
    }
}
