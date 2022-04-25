using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using Mzr.Share.Models.Bilibili;

namespace Mzr.Share.Interfaces.Bilibili
{
    public interface IBiliRepository<T> : IDisposable where T : BiliBase
    {
        IMongoCollection<T> Collection { get; }
        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);

        Task UpsertAsync(T entity);

        Task DeleteAsync(ObjectId id);

        Task DeleteAsync(string id);

        Task<T> GetAsync(ObjectId id);

        Task<T> GetAsync(string id);
    }
}
