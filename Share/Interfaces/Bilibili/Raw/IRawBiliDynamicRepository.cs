using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliDynamicRepository : IRawBiliRepository<RawBiliDynamic>
    {
        Task<BiliDynamic?> GetBiliDynamicAsync(RawBiliDynamic raw);
        Task<BiliDynamic?> FromCardDocument(RawBiliDynamicDataCard cardDocument);
    }
}
