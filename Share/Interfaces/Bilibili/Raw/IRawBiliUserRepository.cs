using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliUserRepository : IRawBiliRepository<RawBiliUser>
    {
        Task<BiliUser?> GetBiliUserAsync(RawBiliUser raw);
    }
}
