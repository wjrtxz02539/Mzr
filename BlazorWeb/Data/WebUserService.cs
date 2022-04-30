using BlazorWeb.Models.Web;
using BlazorWeb.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BlazorWeb.Data
{
    public class WebUserService
    {
        public bool IsBanned => webUser?.Banned ?? true;

        private readonly WebUserRepository webUserRepo;
        private WebUser? webUser;
        private ILogger logger;

        public WebUserService(WebUserRepository webUserRepo, ILogger<WebUserService> logger)
        {
            this.webUserRepo = webUserRepo;
            this.logger = logger;
        }


        public async Task Init(string? username)
        {
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
    }
}
