using Crnc.Oms.Security.Domain.Aggregates;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Domain.SeedWork;
using MongoDB.Bson.Serialization;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Mappings
{
    public static class MongoDbMapping
    {

        public static void RegisterAllMappings()
        {
            RegisterDomainEntityMapping();
            RegisterUserMapping();
            RegisterRoleMapping();
        }

        private static void RegisterDomainEntityMapping()
        {
            BsonClassMap.RegisterClassMap<DomainEntity>(cm =>  {
                cm.AutoMap();
                cm.MapIdMember(f => f.Id);
                cm.SetIgnoreExtraElements(true);
            });
        }

        private static void RegisterUserMapping()
        {
            BsonClassMap.RegisterClassMap<User>(cm =>  {
                cm.AutoMap();                       
            });
        }

        private static void RegisterRoleMapping()
        {
            BsonClassMap.RegisterClassMap<Role>(cm =>  {
                cm.AutoMap();
            });
        }
    }
}