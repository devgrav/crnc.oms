using MongoDB.Bson.Serialization.Conventions;

namespace Crnc.Oms.DataAccess.Mappings
{
    public static class MongoDbConvention
    {
        public static void RegisterConventions()
        {
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register("OmsDbConvention",pack,t => true);  
        }
    }
}