using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Models.Bilibili;
using Mzr.Web.Models;
using Mzr.Web.Models.Configurations;
using Mzr.Web.Services;

namespace Mzr.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string QueryType { get; set; } = string.Empty;
        [BindProperty]
        public string QueryString { get; set; } = string.Empty;

        private readonly ILogger<IndexModel> _logger;

        public readonly GlobalStats GlobalStats;

        public IndexModel(ILogger<IndexModel> logger, GlobalStats global)
        {
            _logger = logger;
            this.GlobalStats = global;
        }

        public void OnGet()
        {
            
        }
    }
}