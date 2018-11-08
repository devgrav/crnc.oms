using System.Collections.Generic;
using Crnc.Oms.Domain.Aggregates.Users;
using MongoDB.Driver;

namespace Crnc.Oms.DataAccess
{
    public class MongoDataContext
    {
        private readonly IMongoDatabase _database;

        public MongoDataContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.Server);
            _database = client.GetDatabase(settings.Database);            
        }

        IMongoCollection<User> Users => _database.GetCollection<User>("users");

        IMongoCollection<Role> Roles => _database.GetCollection<Role>("roles");
    }
}