using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mzr.Share.Models.Bilibili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Mzr.Share.Utils;
using Mzr.Share.Models.Bilibili.Raw;
using Mzr.Share.Repositories.Bilibili.Raw;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Interfaces.Bilibili;

namespace Mzr.Share.Repositories.Bilibili.Web
{
    public class WebBiliUserRepository
    {
        private const string accountUrl = @"http://api.bilibili.com/x/space/acc/info?mid={0}&jsonp=jsonp";
        private readonly Request request;
        private readonly IRawBiliUserRepository rawBiliUserRepository;
        private readonly IBiliUserRepository biliUserRepository;
        private readonly ILogger logger;
        public WebBiliUserRepository(IHost host)
        {
            logger = host.Services.GetRequiredService<ILogger<WebBiliUserRepository>>();
            request = host.Services.GetRequiredService<Request>();
            rawBiliUserRepository = host.Services.GetRequiredService<IRawBiliUserRepository>();
            biliUserRepository = host.Services.GetRequiredService<IBiliUserRepository>();
        }
        public async Task<BiliUser?> FromIdAsync(long id, int retryCount = 100)
        {
            RawBiliUser? rawBiliUser;
            var count = 0;
            do
            {
                rawBiliUser = await request.GetFromJsonAsync<RawBiliUser>(string.Format(accountUrl, id), autoHttps: true);

                if (rawBiliUser == null || count >= retryCount)
                    return null;

                count++;
            } while (rawBiliUser.Code != 0);

            var user = await rawBiliUserRepository.GetBiliUserAsync(rawBiliUser);
            if (user == null)
                return null;

            if (biliUserRepository.Collection.Find(f => f.Id == user.Id).Limit(1).FirstOrDefaultAsync() == null)
                await biliUserRepository.InsertAsync(user);
            return user;
        }
    }
}
