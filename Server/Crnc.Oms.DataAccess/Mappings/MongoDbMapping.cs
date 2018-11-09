using Crnc.Oms.Domain.Aggregates;
using Crnc.Oms.Domain.Aggregates.Users;
using MongoDB.Bson.Serialization;

namespace Crnc.Oms.DataAccess.Mappings
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