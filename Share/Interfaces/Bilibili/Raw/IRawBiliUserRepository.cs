using Mzr.Share.Models.Bilibili.Raw;
using Mzr.Share.Models.Bilibili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliUserRepository : IRawBiliRepository<RawBiliUser>
    {
        Task<BiliUser?> GetBiliUserAsync(RawBiliUser raw);
    }
}
