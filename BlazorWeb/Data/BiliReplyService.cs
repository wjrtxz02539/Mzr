using BlazorWeb.Models.Chart;
using BlazorWeb.Models.Web;
using BlazorWeb.Workers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using System.Runtime.CompilerServices;

namespace BlazorWeb.Data
{
    public class BiliReplyService
    {
        private readonly IBiliReplyRepository replyRepo;
        private readonly WebFileService fileService;
        private readonly StatusService statusService;
        public BiliReplyService(IBiliReplyRepository replyRepo,
            WebFileService fileService, StatusService statusService)
        {
            this.replyRepo = replyRepo;
            this.fileService = fileService;
            this.statusService = statusService;
        }

        public FilterDefinition<BiliReply> FilterBuilder(long? userId = null, long? threadId = null, long? upId = null, long? dialogId = null,
            string? contentQuery = null, DateTime? startTime = null, DateTime? endTime = null, long? root = null, long? parent = null, string? ipQuery = null)
        {
            var filterBuilder = Builders<BiliReply>.Filter;
            FilterDefinition<BiliReply> filter = new BsonDocument();
            if (userId.HasValue)
                filter &= filterBuilder.Eq(f => f.UserId, userId);
            if (threadId.HasValue)
                filter &= filterBuilder.Eq(f => f.ThreadId, threadId);
            if (upId.HasValue)
                filter &= filterBuilder.Eq(f => f.UpId, upId);
            if (dialogId.HasValue)
                filter &= filterBuilder.Eq(f => f.Dialog, dialogId);
            if (!string.IsNullOrEmpty(contentQuery))
                filter &= filterBuilder.Regex(f => f.Content, new BsonRegularExpression($"/.*{contentQuery}.*/i "));
            if (root.HasValue)
                filter &= filterBuilder.Eq(f => f.Root, root);
            if (parent.HasValue)
                filter &= filterBuilder.Eq(f => f.Parent, parent);
            if (startTime.HasValue)
                filter &= filterBuilder.Gte(f => f.Time, startTime.Value.ToUniversalTime());
            if (endTime.HasValue)
                filter &= filterBuilder.Lte(f => f.Time, endTime.Value.ToUniversalTime());
            if (!string.IsNullOrEmpty(ipQuery))
                filter &= filterBuilder.Eq(f => f.IP, ipQuery);
            return filter;
        }

        public SortDefinition<BiliReply> GetSortDefinition(string sort)
        {
            var builder = Builders<BiliReply>.Sort;
            SortDefinition<BiliReply> sortDefinition = sort switch
            {
                "time" => builder.Ascending(f => f.Time),
                "-time" => builder.Descending(f => f.Time),
                "user_id" => builder.Ascending(f => f.UserId),
                "-user_id" => builder.Descending(f => f.UserId),
                "like" => builder.Ascending(f => f.Like),
                "-like" => builder.Descending(f => f.Like),
                "replies_count" => builder.Ascending(f => f.RepliesCount),
                "-replies_count" => builder.Descending(f => f.RepliesCount),
                "ip" => builder.Ascending(f => f.IP),
                "-ip" => builder.Descending(f => f.IP),
                _ => new BsonDocument(),
            };
            return sortDefinition;
        }

        public async Task<List<Tuple<int, long, BiliUser?>>> GetTopUsersAsync(DateTime? startTime = null, DateTime? endTime = null, long? upId = null,
            long? threadId = null, int limit = 10, CancellationToken cancellationToken = default)
        {
            var builder = Builders<BiliReply>.Filter;
            var filter = FilterBuilder(upId: upId, threadId: threadId, startTime: startTime, endTime: endTime);

            PipelineDefinition<BiliReply, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filter.Render(replyRepo.Collection.DocumentSerializer, replyRepo.Collection.Settings.SerializerRegistry)),
                BsonDocument.Parse("{$group: {_id: \"$user_id\", count: {$sum: 1}}}"),
                BsonDocument.Parse("{$sort: {\"count\": -1}}"),
                BsonDocument.Parse($"{{$limit: {limit}}}"),
                BsonDocument.Parse("{$lookup: {\"from\": \"bili_user\", \"localField\": \"_id\", \"foreignField\": \"user_id\", \"as\": \"user\"}}")
            };

            var results = await replyRepo.Collection.Aggregate(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            var result = new List<Tuple<int, long, BiliUser?>>();
            foreach (var item in results)
            {
                var userItem = item["user"].AsBsonArray.FirstOrDefault();
                Tuple<int, long, BiliUser?> tuple;
                if (userItem != null)
                {
                    var user = BsonSerializer.Deserialize<BiliUser>(userItem.AsBsonDocument);
                    tuple = new(item["count"].AsInt32, item["_id"].AsInt64, user);
                }
                else
                {
                    tuple = new(item["count"].AsInt32, item["_id"].AsInt64, null);
                }
                result.Add(tuple);
            }

            return result;
        }


        public async Task<PagingResponse<BiliReply>> PaginationAsync(long? userId = null, long? threadId = null, long? upId = null, long? dialogId = null,
            int page = 1, int size = 0, string sort = "-time", string? contentQuery = null, DateTime? startTime = null, DateTime? endTime = null,
            long? root = null, long? parent = null, string? ipQuery = null, CancellationToken cancellation = default)
        {
            var filter = FilterBuilder(userId: userId, threadId: threadId, upId: upId, dialogId: dialogId,
                contentQuery: contentQuery, startTime: startTime, endTime: endTime, root: root, parent: parent, ipQuery: ipQuery);

            var sortDefinition = GetSortDefinition(sort);
            var result = await replyRepo.Collection.Find(filter).Limit(size).Skip((page - 1) * size).Sort(sortDefinition).ToListAsync(cancellation);
            var totalCount = (int)await replyRepo.Collection.CountDocumentsAsync(filter, cancellationToken: cancellation);
            return new PagingResponse<BiliReply>(result, totalCount: totalCount, pageSize: size, currentPage: page);
        }

        public async Task<List<TimeLineValue>> TimeGroupAsync(long? userId = null, long? threadId = null, long? upId = null,
            string? contentQuery = null, string? ipQuery = null, DateTime? startTime = null, DateTime? endTime = null, CancellationToken cancellationToken = default)
        {
            var filter = FilterBuilder(userId: userId, threadId: threadId, upId: upId, contentQuery: contentQuery, startTime: startTime, endTime: endTime, ipQuery: ipQuery);
            var filterBson = filter.Render(replyRepo.Collection.DocumentSerializer, replyRepo.Collection.Settings.SerializerRegistry);

            PipelineDefinition<BiliReply, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filterBson),
                BsonDocument.Parse("{$project: {year: {$year: {date: \"$time\", timezone: \"Asia/Shanghai\"}}, month: {$month: {date: \"$time\", timezone: \"Asia/Shanghai\"}}, day: {$dayOfMonth: {date: \"$time\", timezone: \"Asia/Shanghai\"}}, hour: { $hour: {date: \"$time\", timezone: \"Asia/Shanghai\"}} }}"),
                BsonDocument.Parse("{$group: {_id: {year: \"$year\", month: \"$month\", day: \"$day\", hour: \"$hour\"}, count: {$sum: 1}}}"),
                BsonDocument.Parse("{$project: {_id: 0, count: 1, time: {$dateFromParts: {year: \"$_id.year\", month: \"$_id.month\", day: \"$_id.day\", hour: \"$_id.hour\", timezone: \"Asia/Shanghai\"}}}}"),
                BsonDocument.Parse("{$sort: {\"time\": 1}}")
            };

            var result = new List<TimeLineValue>();
            await replyRepo.Collection.Aggregate(pipeline, cancellationToken: cancellationToken).ForEachAsync(item =>
            {
                result.Add(new()
                {
                    UtcTime = item["time"].ToUniversalTime(),
                    Count = item["count"].AsInt32
                });
            }, cancellationToken: cancellationToken);

            return result;
        }

        public async IAsyncEnumerable<BiliReply> ExportAsync(long? userId = null, long? threadId = null, long? upId = null, long? dialogId = null,
            int skip = 1, int size = 0, string sort = "-time", string? contentQuery = null, DateTime? startTime = null, DateTime? endTime = null,
            long? root = null, long? parent = null, string? ipQuery = null, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var filter = FilterBuilder(userId: userId, threadId: threadId, upId: upId, dialogId: dialogId,
                contentQuery: contentQuery, startTime: startTime, endTime: endTime, root: root, parent: parent, ipQuery: ipQuery);

            var sortDefinition = GetSortDefinition(sort);
            var cursor = await replyRepo.Collection.Find(filter).Limit(size).Skip(skip).Sort(sortDefinition).ToCursorAsync(cancellation);
            while (await cursor.MoveNextAsync(cancellation))
            {
                foreach (var item in cursor.Current)
                    yield return item;
            }
        }

        public async Task<WebFile?> SubmitExportTask(string username, long? userId = null, long? threadId = null, long? upId = null, long? dialogId = null,
            int skip = 1, int size = 0, string sort = "-time", string? contentQuery = null, DateTime? startTime = null, DateTime? endTime = null,
            long? root = null, long? parent = null, string? ipQuery = null, DateTime? expiredTime = null)
        {
            var parameters = new Dictionary<string, object?>()
            {
                ["userId"] = userId,
                ["threadId"] = threadId,
                ["upId"] = upId,
                ["dialogId"] = dialogId,
                ["skip"] = skip,
                ["size"] = size,
                ["sort"] = sort,
                ["contentQuery"] = contentQuery,
                ["startTime"] = startTime,
                ["endTime"] = endTime,
                ["root"] = root,
                ["parent"] = parent,
                ["ipQuery"] = ipQuery
            };

            var file = await fileService.AddAsync(
                filename: $"{username}_{DateTime.Now:yyyy-MM-dd_HH:mm:ss}.zip",
                username: username,
                function: WebFileFunction.ReplyExport,
                parameters: parameters,
                createdTime: DateTime.UtcNow,
                expiredTime: expiredTime ?? DateTime.UtcNow.AddDays(30)
                );

            if (statusService.SubmitExportJob(file))
                return file;
            else
            {
                await fileService.DeleteAsync(file);
                return null;
            }
        }

        public async Task<List<Tuple<BiliUser, long>>> ReplyCountGroupByUpAsync(long? userId = null, long? threadId = null, long? upId = null, long? dialogId = null,
            string? contentQuery = null, DateTime? startTime = null, DateTime? endTime = null, long? root = null, long? parent = null, string? ipQuery = null, CancellationToken cancellationToken = default)
        {
            var filter = FilterBuilder(userId: userId, threadId: threadId, upId: upId, dialogId: dialogId,
                contentQuery: contentQuery, startTime: startTime, endTime: endTime, root: root, parent: parent, ipQuery: ipQuery);

            var filterBson = filter.Render(replyRepo.Collection.DocumentSerializer, replyRepo.Collection.Settings.SerializerRegistry);

            PipelineDefinition<BiliReply, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filterBson),
                BsonDocument.Parse("{$group: {_id: \"$up_id\", count: {$sum: 1}}}"),
            };

            var result = new List<Tuple<BiliUser, long>>();
            await replyRepo.Collection.Aggregate(pipeline, cancellationToken: cancellationToken).ForEachAsync(item =>
            {
                var up = statusService.MonitoredUp.GetValueOrDefault(item["_id"].AsInt64);
                if (up == null)
                    return;
                result.Add(new(up, item["count"].AsInt32));
            }, cancellationToken: cancellationToken);

            return result;
        }
    }
}
