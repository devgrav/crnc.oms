using System.Threading.Tasks;
using MongoDB.Driver;

namespace Crnc.Oms.DataAccess.Data
{
    public class MongoDbInitializer
    {
        private readonly MongoClient _client;
        private readonly string _dbName;

        public MongoDbInitializer(MongoClient client, string dbName)
        {
            _client = client;
            _dbName = dbName;
        }

        public void Initialize()
        {
            var isExist = IsDatabaseExistAsync().GetAwaiter().GetResult(); 
            if(isExist)
                return;            
        }

        private void FillUsers()
        {
            var user = DataFactory.GetUsers();

        }

        private void FillRoles()
        {
            var user = DataFactory.GetRoles();
        }

        private async Task<bool> IsDatabaseExistAsync()
        {
            using(var cursor =  await _client.ListDatabaseNamesAsync()){
                var dbNames = await cursor.ToListAsync();
                return dbNames.Contains(_dbName);                    
            }
        }
    }
}