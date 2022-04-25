using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models.Web;
using Mzr.Web.Services;

namespace Mzr.Web.Controllers
{
    [ApiController]
    [Route("/api/dynamic")]
    public class DynamicController : Controller
    {
        private readonly BiliDynamicService dynamicService;

        public DynamicController(BiliDynamicService dynamicService)
        {
            this.dynamicService = dynamicService;
        }
        [HttpGet("/api/dynamic/user/{userId:required}")]
        public async Task<PagingResponse<BiliDynamic>> GetByUserIdAsync(long userId, int page = 0, int size = 10, string sort = "-time")
        {
            return await dynamicService.Pagination(userId: userId, page: page, size: size, sort: sort);
        }
    }
}
