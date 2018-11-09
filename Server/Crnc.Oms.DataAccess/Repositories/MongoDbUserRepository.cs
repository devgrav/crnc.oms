using System;
using System.Collections.Generic;
using Crnc.Oms.DataAccess.Exceptions;
using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Crnc.Oms.DataAccess.Repositories
{
    public class MongoDbUserRepository
        : IUserRepository
    {
        private MongoDataContext _dbContext;

        public MongoDbUserRepository(MongoDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User FindByLogin(string login)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Role> GetRoles()
        {
            return _dbContext.Roles.AsQueryable().ToList();
        }

        #region IRepository 

        public void Add(User entity)
        {
            throw new NotImplementedException();            
        }

        public void Delete(Guid entityId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> FindAll()
        {
            return _dbContext.Users.AsQueryable().ToList();
        }

        public User FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save(User entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}