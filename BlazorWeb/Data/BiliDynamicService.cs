using BlazorWeb.Models.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;

namespace BlazorWeb.Data
{
    public class BiliDynamicService
    {
        private readonly IBiliDynamicRepository dynamicRepo;
        private readonly IBiliDynamicRunRecordRepository runRecordRepo;
        public BiliDynamicService(IBiliDynamicRepository dynamicRepo, IBiliDynamicRunRecordRepository runRecordRepo)
        {
            this.dynamicRepo = dynamicRepo;
            this.runRecordRepo = runRecordRepo;
        }

        public async Task<PagingResponse<BiliDynamic>> PaginationAsync(long? userId = null, int page = 2, int size = 10, string sort = "-time",
            string? descriptionQuery = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            var sortBuilder = Builders<BiliDynamic>.Sort;
            SortDefinition<BiliDynamic> sortDefinition = sort switch
            {
                "time" => sortBuilder.Ascending(f => f.Time),
                "-time" => sortBuilder.Descending(f => f.Time),
                "view" => sortBuilder.Ascending(f => f.View),
                "-view" => sortBuilder.Descending(f => f.View),
                "like" => sortBuilder.Ascending(f => f.Like),
                "-like" => sortBuilder.Descending(f => f.Like),
                _ => new BsonDocument(),
            };
            var filterBuilder = Builders<BiliDynamic>.Filter;
            FilterDefinition<BiliDynamic> filter = new BsonDocument();

            if (userId.HasValue)
                filter &= filterBuilder.Eq(f => f.UserId, userId);

            if (!string.IsNullOrEmpty(descriptionQuery))
                filter &= filterBuilder.Regex(f => f.Description, new BsonRegularExpression(descriptionQuery));

            if (startTime.HasValue)
                filter &= filterBuilder.Gte(f => f.Time, startTime.Value);

            if(endTime.HasValue)
                filter &= filterBuilder.Lte(f => f.Time, endTime.Value);

            var result = await dynamicRepo.Collection.Find(filter).Limit(size).Skip((page - 1) * size).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await dynamicRepo.Collection.CountDocumentsAsync(filter);
            return new() { Items = result, MetaData = new() { CurrentPage = page, PageSize = size, TotalCount = totalCount } };
        }

        public async Task<BiliDynamic?> GetByDynamicId(long dynamicId)
        {
            return await dynamicRepo.Collection.Find(f => f.DynamicId == dynamicId).FirstOrDefaultAsync();
        }

        public async Task<BiliDynamic?> GetByThreadId(long threadId)
        {
            return await dynamicRepo.Collection.Find(f => f.ThreadId == threadId).FirstOrDefaultAsync();
        }

        public async Task<BiliDynamicRunRecord> GetLatestRunRecord(long dynamicId, bool onlySuccess = true)
        {
            var builder = Builders<BiliDynamicRunRecord>.Filter;
            var filter = builder.Eq(f => f.DynamicId, dynamicId);

            if (onlySuccess)
                filter &= builder.Ne(f => f.EndTime, null);

            return await runRecordRepo.Collection.Find(filter).SortByDescending(f => f.EndTime).FirstOrDefaultAsync();
        }
    }
}
