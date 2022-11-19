using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Models;

namespace Mzr.Share.Interfaces
{
    public interface IMongoDBRepository<T> : IDisposable where T : MongoDBBase
    {
        IMongoCollection<T> Collection { get; }

        FilterDefinitionBuilder<T> Filter { get; }
        UpdateDefinitionBuilder<T> Update { get; }
        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);

        Task UpsertAsync(T entity);

        Task DeleteAsync(ObjectId id);

        Task DeleteAsync(string id);

        Task<T> GetAsync(ObjectId id);

        Task<T> GetAsync(string id);
    }
}
