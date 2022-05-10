using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Mzr.Share.Interfaces;
using Mzr.Share.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories
{
    public class MongoDBRepository<T> : IMongoDBRepository<T> where T : MongoDBBase
    {
        public ILogger Logger { get; }
        public IMongoDatabase? Database { get; }

        private readonly string CollectionName;
        private readonly List<CreateIndexModel<T>> _indexModels;
        private IMongoCollection<T>? _collection = null;

        public IMongoCollection<T> Collection
        {
            get
            {
                if (_collection == null)
                {
                    if (Database == null)
                        throw new Exception($"Database {Database?.DatabaseNamespace} is null.");

                    if (!Database.ListCollectionNames().ToList().Contains(CollectionName))
                        Database.CreateCollection(CollectionName);

                    _collection = Database?.GetCollection<T>(CollectionName);
                    if (_indexModels.Count > 0)
                        _collection?.Indexes.CreateMany(_indexModels);
                }

                if (_collection == null)
                    throw new Exception($"Collection {CollectionName} not found.");
                return _collection;
            }
        }

        public FilterDefinitionBuilder<T> Filter => Builders<T>.Filter;
        public UpdateDefinitionBuilder<T> Update => Builders<T>.Update;

        public MongoDBRepository(IMongoDatabase mongoDatabase, ILogger logger, string collectionName, List<CreateIndexModel<T>>? createIndexModels = null)
        {
            CollectionName = collectionName;

            Logger = logger;
            Database = mongoDatabase;

            if (createIndexModels == null)
                createIndexModels = new List<CreateIndexModel<T>>();
            _indexModels = createIndexModels;
        }

        public async Task InsertAsync(T entity) =>
            await Collection.InsertOneAsync(entity);

        public async Task UpdateAsync(T entity) =>
            await Collection.ReplaceOneAsync(f => f.Id == entity.Id, entity);

        public async Task UpsertAsync(T entity) =>
            await Collection.ReplaceOneAsync(f => f.Id == entity.Id, entity, new ReplaceOptions() { IsUpsert = true });

        public async Task DeleteAsync(ObjectId id) =>
            await Collection.DeleteOneAsync(f => f.Id == id);

        public async Task DeleteAsync(string id) =>
            await Collection.DeleteOneAsync(f => f.Id == ObjectId.Parse(id));

        public async Task<T> GetAsync(ObjectId id) =>
            await Collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task<T> GetAsync(string id) =>
            await GetAsync(ObjectId.Parse(id));

        public void Dispose()
        {
            Console.WriteLine($"{CollectionName} disposed.");
        }
    }
}
