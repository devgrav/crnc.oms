using System.Collections.Generic;
using System.Threading.Tasks;
using Crnc.Oms.Domain.Aggregates.Users;
using MongoDB.Driver;
using Crnc.Oms.DataAccess.Data;

namespace Crnc.Oms.DataAccess
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
                _client.DropDatabase(_dbName);  

            //Порядок имеет значение, иначе будут неверно сгенерированы ссылки
            var roles = FillRoles();
            FillUsers(roles);                     
        }

        private void FillUsers(List<Role> roles)
        {
            var users = DataFactory.GetUsers(roles);
            var usersMongo = _client.GetDatabase(_dbName).GetCollection<User>("users");
            usersMongo.InsertMany(users);
        }

        private List<Role> FillRoles()
        {
            var roles = DataFactory.GetRoles();
            var rolesMongo = _client.GetDatabase(_dbName).GetCollection<Role>("roles");
            rolesMongo.InsertMany(roles);
            return roles;
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