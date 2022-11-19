using Mzr.Share.Models.Bilibili.Raw;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliRepository<T> : IDisposable where T : RawBiliBase
    {
    }
}
