using BlazorWeb.Models.Web;
using BlazorWeb.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BlazorWeb.Data
{
    public class WebUserService
    {
        public bool IsBanned => webUser?.Banned ?? false;

        private readonly WebUserRepository webUserRepo;
        private WebUser? webUser;
        private ILogger logger;
        private WebLogRepository logRepo;

        public WebUserService(WebUserRepository webUserRepo, ILogger<WebUserService> logger, WebLogRepository logRepo)
        {
            this.webUserRepo = webUserRepo;
            this.logger = logger;
            this.logRepo = logRepo;
        }


        public async Task Init(string? username)
        {
            if (webUser != null)
                return;

            if (username == null)
            {
                logger.LogWarning("username is null.");
                return;
            }

            var response = await webUserRepo.Collection.Find(f => f.Username == username).FirstOrDefaultAsync();
            if(response == null)
            {
                var newUser = new WebUser
                {
                    Username = username,
                    RegisterTime = DateTime.UtcNow,
                    LoginTime = DateTime.UtcNow,
                    VisitCount = 1,
                    VisitTime = DateTime.UtcNow,
                };
                await webUserRepo.InsertAsync(newUser);
                logger.LogInformation("Welcome {name}.", username);
            }
            else
            {
                webUser = response;
                webUser.LoginTime = DateTime.UtcNow;
                webUser.VisitTime = DateTime.UtcNow;
                webUser.VisitCount++;
                await webUserRepo.UpdateAsync(webUser);
            }
        }

        public async Task Log(string function, Dictionary<string, object?> parameters, string status, long elapsed = 0, 
            string url = "", string error = "", string username = "")
        {
            if (string.IsNullOrEmpty(username))
                username = webUser?.Username ?? string.Empty;

            var log = new WebLog
            {
                Function = function,
                Parameters = parameters,
                Status = status,
                Username = username,
                Elasped = elapsed,
                Url = url,
                Error = error,
                Time = DateTime.UtcNow,
            };

            await logRepo.InsertAsync(log);
        }
    }
}
