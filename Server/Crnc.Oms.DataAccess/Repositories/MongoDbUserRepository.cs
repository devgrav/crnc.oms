using System.Collections.Generic;
using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;

namespace Crnc.Oms.DataAccess.Repositories
{
    public class MongoDbUserRepository
        : IUserRepository
    {
        private readonly MongoDataContext _dbContext;

        public MongoDbUserRepository(MongoDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(User entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int entityId)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<User> FindAll()
        {
            throw new System.NotImplementedException();
        }

        public User FindById(int id)
        {
            throw new System.NotImplementedException();
        }

        public User FindByLogin(string login)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Role> GetRoles()
        {
            throw new System.NotImplementedException();
        }

        public void Save(User entity)
        {
            throw new System.NotImplementedException();
        }
    }
}