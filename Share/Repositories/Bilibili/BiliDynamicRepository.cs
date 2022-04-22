using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using Mzr.Share.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliDynamicRepository : BiliRepository<BiliDynamic>, IBiliDynamicRepository
    {
        public BiliDynamicRepository(IHost host, ILogger<BiliDynamicRepository> logger) : base(
            host, 
            logger,
            "bili_dynamic",
            new List<CreateIndexModel<BiliDynamic>>()
            {
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.DynamicId), new CreateIndexOptions(){Background = true, Unique=true}),
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.ThreadId), new CreateIndexOptions(){Background = true}),
                new CreateIndexModel<BiliDynamic>(Builders<BiliDynamic>.IndexKeys.Ascending(f => f.UserId), new CreateIndexOptions(){Background = true})
            })
        {
        }
    }
}
