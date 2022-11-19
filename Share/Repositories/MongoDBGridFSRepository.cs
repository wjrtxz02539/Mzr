using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Mzr.Share.Interfaces;
using Mzr.Share.Models;

namespace Mzr.Share.Repositories
{
    public class MongoDBGridFSRepository<T> : MongoDBRepository<T>, IMongoDBGridFSRepository where T : MongoDBBase
    {
        private readonly string bucketName;
        private IGridFSBucket? _bucket = null;

        public IGridFSBucket Bucket
        {
            get
            {
                if (_bucket == null)
                {
                    if (Database == null)
                        throw new Exception($"Database {Database?.DatabaseNamespace} is null.");

                    _bucket = new GridFSBucket(Database, new()
                    {
                        BucketName = bucketName,
                        ReadPreference = ReadPreference.SecondaryPreferred
                    });
                }

                if (_bucket == null)
                    throw new Exception($"GridFS bucket {bucketName} not found.");
                return _bucket;
            }
        }
        public MongoDBGridFSRepository(IMongoDatabase mongoDatabase, ILogger logger, string bucketName) : base(
            mongoDatabase, logger, bucketName)
        {
            this.bucketName = bucketName;
        }
    }
}
