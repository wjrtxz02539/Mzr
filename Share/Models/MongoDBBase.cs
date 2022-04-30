using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Models
{
    public class MongoDBBase
    {
        public ObjectId Id { get; set; }
    }
}
