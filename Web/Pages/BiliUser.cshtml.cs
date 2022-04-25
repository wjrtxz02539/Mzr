using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models;

namespace Mzr.Web.Pages
{
    public class BiliUserModel : PageModel
    {
        public long UserId { get; set; }
        public bool ReplyCountEnabled { get; set; }

        public IActionResult OnGet(long userId, bool replyCountEnabled = true)
        {
            UserId = userId;
            ReplyCountEnabled = replyCountEnabled;
            return Page();
        }
    }
}
