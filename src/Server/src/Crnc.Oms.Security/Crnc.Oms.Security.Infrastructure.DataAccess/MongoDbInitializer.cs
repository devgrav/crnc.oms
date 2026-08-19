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

        public async Task InitializeAsync()
        {
            var dbName = _dataContext.Database.DatabaseNamespace.DatabaseName;
            try
            {
                var isExist = await IsDatabaseExistAsync();
                if(isExist)
                    await _dataContext.Client.DropDatabaseAsync(dbName);

                //Порядок имеет значение, иначе будут неверно сгенерированы ссылки
                var roles = await FillRolesAsync();
                await FillUsersAsync(roles);
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

        private async Task FillUsersAsync(List<Role> roles)
        {
            var users = DataFactory.GetUsers(roles);
            await _dataContext.Users.InsertManyAsync(users);
        }

        private async Task<List<Role>> FillRolesAsync()
        {
            var roles = DataFactory.GetRoles();
            await _dataContext.Roles.InsertManyAsync(roles);
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
