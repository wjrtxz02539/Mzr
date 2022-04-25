using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models.Web;

namespace Mzr.Web.Services
{
    public class BiliReplyService
    {
        private readonly ILogger<BiliReplyService> logger;
        private readonly IBiliReplyRepository replyRepo;
        private readonly IBiliDynamicRepository dynamicRepo;
        public BiliReplyService(ILogger<BiliReplyService> logger, IBiliReplyRepository replyRepo, IBiliDynamicRepository dynamicRepo)
        {
            this.logger = logger;
            this.replyRepo = replyRepo;
            this.dynamicRepo = dynamicRepo;
        }

        public async Task<List<Tuple<int, long, BiliUser?>>> GetTopUsersAsync(DateTime startTime, DateTime endTime, long? upId = null,
            long? dynamicId = null, long? threadId = null, int limit = 10, CancellationToken cancellationToken = default)
        {
            var builder = Builders<BiliReply>.Filter;
            var filter = builder.Gte(f => f.Time, startTime.ToUniversalTime()) & builder.Lte(f => f.Time, endTime.ToUniversalTime());

            if (upId.HasValue)
                filter &= builder.Eq(f => f.UpId, upId);

            if (dynamicId.HasValue)
            {
                var dynamic = await dynamicRepo.Collection.Find(f => f.DynamicId == dynamicId).FirstOrDefaultAsync();
                if (dynamic != null)
                    threadId = dynamic.ThreadId;
            }

            if (threadId.HasValue)
                filter &= builder.Eq(f => f.ThreadId, threadId);


            PipelineDefinition<BiliReply, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filter.Render(replyRepo.Collection.DocumentSerializer, replyRepo.Collection.Settings.SerializerRegistry)),
                BsonDocument.Parse("{$group: {_id: \"$user_id\", count: {$sum: 1}}}"),
                BsonDocument.Parse("{$sort: {\"count\": -1}}"),
                BsonDocument.Parse($"{{$limit: {limit}}}"),
                BsonDocument.Parse("{$lookup: {\"from\": \"bili_user\", \"localField\": \"_id\", \"foreignField\": \"user_id\", \"as\": \"user\"}}")
            };

            var results = await replyRepo.Collection.Aggregate(pipeline).ToListAsync(cancellationToken);

            var result = new List<Tuple<int, long, BiliUser?>>();
            foreach(var item in results)
            {
                var userItem = item["user"].AsBsonArray.FirstOrDefault();
                Tuple<int, long, BiliUser?> tuple;
                if (userItem != null)
                {
                    var user = BsonSerializer.Deserialize<BiliUser>(userItem.AsBsonDocument);
                    tuple = new (item["count"].AsInt32, item["_id"].AsInt64, user);
                }
                else
                {
                    tuple = new (item["count"].AsInt32, item["_id"].AsInt64, null);
                }
                result.Add(tuple);
            }

            return result;
        }

        public async Task<PagingResponse<BiliReply>> Pagination(long? userId, long? threadId, long? upId, long? dialogId, int page = 1, int size = 0, string sort = "-time")
        {
            var filterBuilder = Builders<BiliReply>.Filter;
            FilterDefinition<BiliReply> filter;
            if (userId.HasValue)
                filter = filterBuilder.Eq(f => f.UserId, userId);
            else if (threadId.HasValue)
                filter = filterBuilder.Eq(f => f.ThreadId, threadId);
            else if (upId.HasValue)
                filter = filterBuilder.Eq(f => f.UpId, upId);
            else if (dialogId.HasValue)
                filter = filterBuilder.Eq(f => f.Dialog, dialogId);
            else
                return new PagingResponse<BiliReply>();

            SortDefinition<BiliReply> sortDefinition;
            var builder = Builders<BiliReply>.Sort;

            switch (sort)
            {
                case "time":
                    sortDefinition = builder.Ascending(f => f.Time);
                    break;
                case "-time":
                    sortDefinition = builder.Descending(f => f.Time);
                    break;
                default:
                    sortDefinition = builder.Descending(f => f.Time);
                    break;
            }
            var result = await replyRepo.Collection.Find(filter).Limit(size).Skip((page - 1) * size).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await replyRepo.Collection.CountDocumentsAsync(filter);
            return new PagingResponse<BiliReply>(result, totalCount: totalCount, pageSize: size, currentPage: page);
        }
    }
}
