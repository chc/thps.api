using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
namespace THPS.API.DbContext
{
    public class THPSAPIDBContext : ITHPSAPIDBContext
    {
        private string uri;
        private MongoClient client;
        public THPSAPIDBContext(string uri)
        {
            this.uri = uri;
            client = new MongoClient(uri);
        }
        public MongoClient GetMongoClient()
        {
            return client;
        }
        public IMongoDatabase GetDatabase()
        {
            return client.GetDatabase("checksums");
        }
    }
}
