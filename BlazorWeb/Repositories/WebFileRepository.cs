using BlazorWeb.Models.Web;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Repositories;

namespace BlazorWeb.Repositories
{
    public class WebFileRepository : MongoDBGridFSRepository<WebFile>
    {
        public WebFileRepository(IMongoDatabase mongoDatabase, ILogger<WebFileRepository> logger) : base (
            mongoDatabase, logger, "web_file")
        { }
    }
}
