using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models.Web;
using Mzr.Web.Services;

namespace Mzr.Web.Controllers
{
    [ApiController]
    [Route("/api/reply")]
    public class ReplyController : Controller
    {
        private readonly BiliReplyService replyService;
        public ReplyController(BiliReplyService replyService)
        {
            this.replyService = replyService;
        }

        [HttpGet("/api/reply/thread/{threadId:required}")]
        public async Task<PagingResponse<BiliReply>> GetByThreadAsync(long threadId, int page = 1, int size = 10, string sort = "-time")
        {
            return await replyService.Pagination(userId: null, threadId: threadId, upId: null, dialogId: null, page: page, size: size, sort: sort);
            
        }

        [HttpGet("/api/reply/user/{userId:required}")]
        public async Task<PagingResponse<BiliReply>> GetByUserAsync(long userId, int page = 1, int size = 10, string sort = "-time")
        {
            return await replyService.Pagination(userId: userId, threadId: null, upId: null, dialogId: null, page: page, size: size, sort: sort);

        }

        [HttpGet("/api/reply/up/{upId:required}")]
        public async Task<PagingResponse<BiliReply>> GetByUpAsync(long upId, int page = 1, int size = 10, string sort = "-time")
        {
            return await replyService.Pagination(userId: null, threadId: null, upId: upId, dialogId: null, page: page, size: size, sort: sort);

        }

        [HttpGet("/api/reply/dialog/{dialogId:required}")]
        public async Task<PagingResponse<BiliReply>> GetByDialogAsync(long dialogId, int page = 1, int size = 10, string sort = "-time")
        {
            return await replyService.Pagination(userId: null, threadId: null, upId: null, dialogId: dialogId, page: page, size: size, sort: sort);

        }
    }
}
