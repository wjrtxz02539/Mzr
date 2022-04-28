using BlazorWeb.Models.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;

namespace BlazorWeb.Data
{
    public class BiliUserService
    {
        private readonly IBiliUserRepository userRepo;
        private readonly IBiliReplyRepository replyRepo;

        public BiliUserService(IBiliUserRepository userRepo, IBiliReplyRepository replyRepo)
        {
            this.userRepo = userRepo;
            this.replyRepo = replyRepo;
        }

        public async Task<PagingResponse<BiliUser>> PaginationAsync(int page = 1, int size = 10, string? sort = null,
            string? usernameQuery = null, string? signQuery = null)
        {
            var sortBuilder = Builders<BiliUser>.Sort;
            SortDefinition<BiliUser> sortDefinition = sort switch
            {
                "user_id" => sortBuilder.Ascending(f => f.UserId),
                "-user_id" => sortBuilder.Descending(f => f.UserId),
                "username" => sortBuilder.Ascending(f => f.Username),
                "-username" => sortBuilder.Descending(f => f.Username),
                "level" => sortBuilder.Ascending(f => f.Level),
                "-level" => sortBuilder.Descending(f => f.Level),
                _ => new BsonDocument(),
            };
            var filterBuilder = Builders<BiliUser>.Filter;
            FilterDefinition<BiliUser> filter = new BsonDocument();

            if (!string.IsNullOrEmpty(usernameQuery))
                filter &= filterBuilder.Regex(f => f.Username, new BsonRegularExpression($"/.*{usernameQuery}.*/i "));

            if (!string.IsNullOrEmpty(signQuery))
                filter &= filterBuilder.Regex(f => f.Sign, new BsonRegularExpression($"/.*{signQuery}.*/i "));

            var result = await userRepo.Collection.Find(filter).Skip((page - 1) * size).Limit(size).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await userRepo.Collection.CountDocumentsAsync(filter);

            return new() { Items = result, MetaData = new() { CurrentPage = page, TotalCount = totalCount, PageSize = size } };
        }

        public async Task<BiliUser?> GetByUserId(long userId)
        {
            return await userRepo.Collection.Find(f => f.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<long> GetUserReplyCount(long userId)
        {
            return await replyRepo.Collection.CountDocumentsAsync(f => f.UserId == userId);
        }
    }
}
