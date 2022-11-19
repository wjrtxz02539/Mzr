using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliDynamicRunRecordRepository : BiliRepository<BiliDynamicRunRecord>, IBiliDynamicRunRecordRepository
    {
        public BiliDynamicRunRecordRepository(IMongoDatabase mongoDatabase, ILogger<BiliDynamicRunRecordRepository> logger) : base(
            mongoDatabase,
            logger,
            "bili_dynamic_run_record",
            new List<CreateIndexModel<BiliDynamicRunRecord>>()
            {
                new CreateIndexModel<BiliDynamicRunRecord>(Builders<BiliDynamicRunRecord>.IndexKeys.Ascending(f => f.DynamicId), new CreateIndexOptions(){Background=true})
            })
        {
        }
    }
}
