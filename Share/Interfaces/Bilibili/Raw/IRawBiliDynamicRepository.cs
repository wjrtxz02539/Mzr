using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliDynamicRepository : IRawBiliRepository<RawBiliDynamic>
    {
        Task<BiliDynamic?> GetBiliDynamicAsync(RawBiliDynamic raw);
        Task<BiliDynamic?> FromCardDocument(RawBiliDynamicDataCard cardDocument);
    }
}
