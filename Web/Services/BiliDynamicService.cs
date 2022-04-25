using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models.Web;
using System.Security.Cryptography.Xml;

namespace Mzr.Web.Services
{
    public class BiliDynamicService
    {
        private readonly IBiliDynamicRepository dynamicRepo;
        public BiliDynamicService(IBiliDynamicRepository dynamicRepo)
        {
            this.dynamicRepo = dynamicRepo;

        }

        public async Task<PagingResponse<BiliDynamic>> Pagination(long userId, int page = 0, int size = 10, string sort = "-time")
        {
            SortDefinition<BiliDynamic> sortDefinition;
            var builder = Builders<BiliDynamic>.Sort;

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
            var result = await dynamicRepo.Collection.Find(f => f.UserId == userId).Limit(size).Skip((page - 1) * size).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await dynamicRepo.Collection.CountDocumentsAsync(f => f.UserId == userId);
            return new() { Items=result, MetaData = new() { CurrentPage=page, PageSize=size, TotalCount=totalCount} };
        }
    }
}
