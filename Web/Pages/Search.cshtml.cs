using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mzr.Web.Pages
{
    public class SearchModel : PageModel
    {
        public string? ReplyQuery { get; set; } = null;
        public string? UsernameQuery { get; set; } = null;
        public string? DynamicQuery { get; set; } = null;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IActionResult OnGet(string queryType, string queryString, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (queryType == "replyQuery")
            {
                ReplyQuery = queryString;
                if (startTime == null || endTime == null)
                {
                    startTime = DateTime.UtcNow.AddDays(-7);
                    endTime = DateTime.UtcNow;
                }
                if (endTime - startTime > TimeSpan.FromDays(30))
                    return BadRequest("Reply query only allow in 30 days.");
            }
            else if (queryType == "usernameQuery")
                UsernameQuery = queryString;
            else if (queryType == "dynamicQuery")
                DynamicQuery = queryString;
            else
                return BadRequest(@"Unknown query type: {queryType}.");

            StartTime = startTime;
            EndTime = endTime;

            return Page();
        }
    }
}
