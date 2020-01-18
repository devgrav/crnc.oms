using System.Collections.Generic;
using System.Text.RegularExpressions;
using Crnc.Oms.Security.Infrastructure.DataAccess.Data;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Infrastructure.DataAccess.Mappings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crnc.Oms.Security.Infrastructure.DataAccess
{
    public class MongoDataContext
    {
        public IMongoDatabase Database { get; private set;}
        
        public MongoClient Client { get; private set;}

        public MongoDataContext(IOptions<MongoDbSettings> settings)
        {
            //Порядок имеет значение, регистрацию конвенций нужно вызывать перед регистрацией маппингов
            //Чтобы использовать nameof для ключей лучше отключить CamelCase
            //MongoDbConvention.RegisterConventions();
            MongoDbMapping.RegisterAllMappings();

            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            Client = new MongoClient(settings.Value.Server);
            Database = Client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<User> Users => Database.GetCollection<User>("users");

        public IMongoCollection<Role> Roles => Database.GetCollection<Role>("roles");
        
        public IMongoCollection<T> Collection<T>(string collectionName) => Database.GetCollection<T>(collectionName);
    }
}