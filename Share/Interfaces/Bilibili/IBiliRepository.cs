using Mzr.Share.Models.Bilibili;

namespace Mzr.Share.Interfaces.Bilibili
{
    public interface IBiliRepository<T> : IDisposable, IMongoDBRepository<T> where T : BiliBase
    {

    }
}
