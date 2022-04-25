using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models.Web;

namespace Mzr.Web.Services
{
    public class BiliDynamicService
    {
        private readonly IBiliDynamicRepository dynamicRepo;
        public BiliDynamicService(IBiliDynamicRepository dynamicRepo)
        {
            this.dynamicRepo = dynamicRepo;
        }

        public async Task<PagingResponse<BiliDynamic>> Pagination(long? userId = null, int page = 2, int size = 10, string sort = "-time",
            string? descriptionQuery = null)
        {
            var sortBuilder = Builders<BiliDynamic>.Sort;
            SortDefinition<BiliDynamic> sortDefinition = sort switch
            {
                "time" => sortBuilder.Ascending(f => f.Time),
                "-time" => sortBuilder.Descending(f => f.Time),
                _ => new BsonDocument(),
            };
            var filterBuilder = Builders<BiliDynamic>.Filter;
            FilterDefinition<BiliDynamic> filter = new BsonDocument();

            if (userId.HasValue)
                filter &= filterBuilder.Eq(f => f.UserId, userId);

            if (!string.IsNullOrEmpty(descriptionQuery))
                filter &= filterBuilder.Regex(f => f.Description, new BsonRegularExpression($"/.*{descriptionQuery}.*/i "));

            var result = await dynamicRepo.Collection.Find(filter).Limit(size).Skip((page - 1) * size).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await dynamicRepo.Collection.CountDocumentsAsync(filter);
            return new() { Items=result, MetaData = new() { CurrentPage=page, PageSize=size, TotalCount=totalCount} };
        }
    }
}
