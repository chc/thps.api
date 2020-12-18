using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace THPS.API.DbContext
{
    public interface ITHPSAPIDBContext
    {
        MongoClient GetMongoClient();
        IMongoDatabase GetDatabase();
    }
}
