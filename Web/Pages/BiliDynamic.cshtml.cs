using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models;

namespace Mzr.Web.Pages
{
    public class BiliDynamicModel : PageModel
    {
        public BiliDynamic? Dynamic { get; set; } = null!;
        public BiliUser? Up { get; set; } = null!;

        private readonly IBiliDynamicRepository dynamicRepo;
        private readonly IBiliUserRepository userRepo;

        public BiliDynamicModel(IBiliDynamicRepository dynamicRepo, IBiliUserRepository userRepo)
        {
            this.dynamicRepo = dynamicRepo;
            this.userRepo = userRepo;
        }
        public async Task<IActionResult> OnGetAsync(long dynamicId, string replySort = "-time")
        {
            Dynamic = await dynamicRepo.Collection.Find(f => f.DynamicId == dynamicId).FirstOrDefaultAsync();
            if (Dynamic == null)
                return NotFound();

            Up = await userRepo.Collection.Find(f => f.UserId == Dynamic.UserId).FirstOrDefaultAsync();
            if(Up == null)
                return NotFound();
            return Page();
        }
    }
}
