using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili.Raw
{
    public class RawBiliUserRepository : RawBiliRepository<RawBiliUser>, IRawBiliUserRepository
    {
        public RawBiliUserRepository(IHost host, ILogger<RawBiliDynamicRepository> logger) : base(logger, host)
        { }
        public async Task<BiliUser?> GetBiliUserAsync(RawBiliUser raw)
        {
            await Task.Yield();
            var user = new BiliUser()
            {
                UserId = raw.Data.UserId,
                UserName = raw.Data.Name,
                Sex = raw.Data.ParsedSex,
                Sign = raw.Data.Sign,
                Level = raw.Data.Level,
                Vip = raw.Data.Vip.Type,
                Avatar = raw.Data.Avatar
            };
            
            if (!string.IsNullOrEmpty(raw.Data.Pendant.Name))
            {
                user.Pendants = new List<string> { raw.Data.Pendant.Name };
            }
            return user;
        }
    }
}
