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
using MongoDB.Bson;

namespace Mzr.Share.Repositories.Bilibili.Raw
{
    public class RawBiliUserRepository : RawBiliRepository<RawBiliUser>, IRawBiliUserRepository
    {
        public RawBiliUserRepository(IHost host, ILogger<RawBiliDynamicRepository> logger) : base(logger, host)
        { }
        public async Task<BiliUser?> GetBiliUserAsync(RawBiliUser raw)
        {
            try
            {
                await Task.Yield();
                var user = new BiliUser()
                {
                    UserId = raw.Data.UserId,
                    Username = raw.Data.Name,
                    Sex = raw.Data.ParsedSex,
                    Sign = raw.Data.Sign,
                    Level = raw.Data.Level,
                    Vip = raw.Data.Vip.Type,
                    Avatar = raw.Data.Avatar,
                    UpdateTime = DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(raw.Data.Pendant.Name))
                {
                    user.Pendants = new List<string> { raw.Data.Pendant.Name };
                }
                return user;
            }
            catch(Exception ex)
            {
                Logger.LogError("{ex}:\n{raw}", ex, raw.ToJson());
            }
            return null;
        }
    }
}
