using System.Collections.Generic;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using MongoDB.Driver;
using System;
using Crnc.Oms.Security.Infrastructure.DataAccess.Data;

namespace Crnc.Oms.Security.Infrastructure.DataAccess
{
    public class MongoDbInitializer
    {
        private readonly MongoDataContext _dataContext;

        public MongoDbInitializer(MongoDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void Initialize()
        {
            var dbName = _dataContext.Database.DatabaseNamespace.DatabaseName;
            try
            {
                var isExist = IsDatabaseExistAsync().GetAwaiter().GetResult(); 
                if(isExist)
                    _dataContext.Client.DropDatabase(dbName);  

                //Порядок имеет значение, иначе будут неверно сгенерированы ссылки
                var roles = FillRoles();
                FillUsers(roles);                      
            }
            catch(TimeoutException e)
            {
                throw new DataAccessException($"Not connected to database {dbName} by timeout, may be database not avaliable", e);
            }
            catch (Exception e)
            {
                throw new DataAccessException($"Not connected to database {dbName}, unexpected error caused", e);
            }               
        }

        private void FillUsers(List<Role> roles)
        {
            var users = DataFactory.GetUsers(roles);
            _dataContext.Users.InsertMany(users);
        }

        private List<Role> FillRoles()
        {
            var roles = DataFactory.GetRoles();
            _dataContext.Roles.InsertMany(roles);
            return roles;
        }

        private async Task<bool> IsDatabaseExistAsync()
        {
            using(var cursor =  await _dataContext.Client.ListDatabaseNamesAsync()){
                var dbNames = await cursor.ToListAsync();
                return dbNames.Contains(_dataContext.Database.DatabaseNamespace.DatabaseName);                    
            }
        }
    }
}