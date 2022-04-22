﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Mzr.Share.Interfaces.Bilibili;
using MongoDB.Driver;
using Mzr.Share.Models.Bilibili;
using MongoDB.Bson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mzr.Share.Configuration;

namespace Mzr.Share.Repositories.Bilibili
{
    public class BiliRepository<T> : IBiliRepository<T> where T : BiliBase
    {
        public ILogger Logger;

        private readonly string CollectionName;
        private readonly List<CreateIndexModel<T>> _indexModels;
        private static IMongoDatabase? Database;
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
                    if (_indexModels.Count() > 0)
                        _collection?.Indexes.CreateMany(_indexModels);
                }

                if (_collection == null)
                    throw new Exception($"Collection {CollectionName} not found.");
                return _collection;
            }
        }

        public BiliRepository(IHost host, ILogger logger, string collectionName, List<CreateIndexModel<T>>? createIndexModels = null)
        {
            CollectionName = collectionName;

            var configuration = host.Services.GetRequiredService<Configuration.Configuration>();
            Logger = logger;

            if (Database is null)
            {
                Database = new MongoClient(configuration.Database.Url).GetDatabase(configuration.Database.DatabaseName);
            }

            if (Database == null)
                throw new Exception($"Database {Database?.DatabaseNamespace} not found.");

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

        public async Task<List<T>> PaginationAsync(FilterDefinition<T> filter, int page, int pageSize) =>
             await Collection.Find(filter).Skip(page * pageSize).Limit(pageSize).ToListAsync();


        public async Task<long> CountAsync(FilterDefinition<T> filter) =>
            await Collection.CountDocumentsAsync(filter);

        public async Task<IAsyncCursor<T>> FindAsync(FilterDefinition<T> filter, FindOptions<T>? options = null) =>
            await Collection.FindAsync(filter, options);

        public void Dispose()
        {
            Console.WriteLine($"{CollectionName} disposed.");
        }
    }
}
