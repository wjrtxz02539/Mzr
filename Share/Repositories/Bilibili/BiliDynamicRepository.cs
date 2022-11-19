using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliDynamicRepository : BiliRepository<BiliDynamic>, IBiliDynamicRepository
    {
        public BiliDynamicRepository(IMongoDatabase mongoDatabase, ILogger<BiliDynamicRepository> logger) : base(
            mongoDatabase,
            logger,
            "bili_dynamic",
            new List<CreateIndexModel<BiliDynamic>>()
            {
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.DynamicId), new CreateIndexOptions(){Background = true, Unique=true}),
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.ThreadId), new CreateIndexOptions(){Background = true}),
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.UserId), new CreateIndexOptions(){Background = true}),
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Descending(f => f.Time), new CreateIndexOptions(){Background = true})
            })
        {
        }
    }
}
