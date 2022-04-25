using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Interfaces.Bilibili;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliUserRepository : BiliRepository<BiliUser>, IBiliUserRepository
    {
        public BiliUserRepository(IMongoDatabase mongoDatabase, ILogger<BiliUserRepository> logger) : base(
            mongoDatabase,
            logger,
            "bili_user", 
            new List<CreateIndexModel<BiliUser>>()
        {
            new CreateIndexModel<BiliUser>(Builders<BiliUser>.IndexKeys.Ascending(f => f.UserId), new CreateIndexOptions() {Unique = true, Background = true}),
            new CreateIndexModel<BiliUser>(Builders<BiliUser>.IndexKeys.Ascending(f => f.Username), new CreateIndexOptions(){Background = true})
        })
        {
        }
    }
}
