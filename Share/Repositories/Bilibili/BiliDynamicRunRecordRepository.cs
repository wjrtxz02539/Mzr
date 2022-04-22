using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliDynamicRunRecordRepository : BiliRepository<BiliDynamicRunRecord>, IBiliDynamicRunRecordRepository
    {
        public BiliDynamicRunRecordRepository(IHost host, ILogger<BiliDynamicRunRecordRepository> logger) : base(
            host,
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
