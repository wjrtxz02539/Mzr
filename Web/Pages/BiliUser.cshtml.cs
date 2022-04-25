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
        public BiliUser User { get; set; } = null!;
        private readonly IBiliUserRepository userRepo;

        public BiliUserModel(IBiliUserRepository userRepo)
        {
            this.userRepo = userRepo;
        }
        public async Task<IActionResult> OnGet(long userId, string replySort = "-time", string dynamicSort = "-time")
        {
            User = await userRepo.Collection.Find(f => f.UserId == userId).FirstOrDefaultAsync();
            if (User == null)
                return NotFound();

            return Page();
        }
    }
}
