using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliReplyRepository : BiliRepository<BiliReply>, IBiliReplyRepository
    {
          public BiliReplyRepository(IHost host, ILogger<BiliReplyRepository> logger) : base(
            host,
            logger,
            "bili_reply",
            new List<CreateIndexModel<BiliReply>>()
            {
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f => f.ReplyId), new CreateIndexOptions(){Unique=true, Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f =>f.ThreadId), new CreateIndexOptions(){Background=true}),
                new CreateIndexModel<BiliReply>(Builders<BiliReply>.IndexKeys.Ascending(f=>f.UpId), new CreateIndexOptions(){Background=true})
            })
        {
        }
    }
}
