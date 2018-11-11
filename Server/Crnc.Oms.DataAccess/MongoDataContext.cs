using System.Collections.Generic;
using Crnc.Oms.DataAccess.Data;
using Crnc.Oms.DataAccess.Mappings;
using Crnc.Oms.Domain.Aggregates.Users;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Crnc.Oms.DataAccess
{
    public class MongoDataContext
    {
        private readonly IMongoDatabase _database;

        public MongoDataContext(IOptions<MongoDbSettings> settings)
        {
            //Порядок имеет значение, регистрацию конвенций нужно вызывать перед регистрацией маппингов
            //TODO: Чтобы использовать nameof для ключей лучше отключить CamelCase
            //MongoDbConvention.RegisterConventions();
            MongoDbMapping.RegisterAllMappings();

            var client = new MongoClient(settings.Value.Server);
            _database = client.GetDatabase(settings.Value.Database);    

            var initializer = new MongoDbInitializer(client, settings.Value.Database);            
            initializer.Initialize();        
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");

        public IMongoCollection<Role> Roles => _database.GetCollection<Role>("roles");
    }
}