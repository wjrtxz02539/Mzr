using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliReplyRepository : BiliRepository<BiliReply>, IBiliReplyRepository
    {
        public BiliReplyRepository(IMongoDatabase mongoDatabase, ILogger<BiliReplyRepository> logger) : base(
          mongoDatabase,
          logger,
          "bili_reply",
          new List<CreateIndexModel<BiliReply>>()
          {
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f => f.ReplyId), new CreateIndexOptions(){Unique=true, Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f =>f.ThreadId), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f=>f.UpId), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f=>f.UserId), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f=>f.Dialog), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Descending(f=>f.Time), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f=>f.IP), new CreateIndexOptions(){Background=true})
          })
        {
        }
    }
}
