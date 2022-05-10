using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.GridFS;

namespace Mzr.Share.Interfaces
{
    public interface IMongoDBGridFSRepository : IDisposable
    {
        IGridFSBucket Bucket { get; }
    }
}
