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
    public interface IBiliRepository<T> : IDisposable, IMongoDBRepository<T> where T : BiliBase
    {
        
    }
}
