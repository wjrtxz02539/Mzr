using MongoDB.Driver.GridFS;

namespace Mzr.Share.Interfaces
{
    public interface IMongoDBGridFSRepository : IDisposable
    {
        IGridFSBucket Bucket { get; }
    }
}
