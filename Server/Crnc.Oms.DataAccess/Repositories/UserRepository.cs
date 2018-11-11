using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.DataAccess.Exceptions;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace Crnc.Oms.DataAccess.Repositories
{
    /// <summary>
    /// Repository for users
    /// </summary>
    public class UserRepository
        : IUserRepository
    {
        private MongoDataContext _dbContext;

        public UserRepository(MongoDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User FindByLogin(string login)
        {
            var user = _dbContext.Users.AsQueryable().SingleOrDefault(u => u.Login == login);

            if (user == null)
                throw new MissingEntityException($"User with such login is not found");

            return user;
        }

        public IEnumerable<Role> GetRoles()
        {
            return _dbContext.Roles.AsQueryable().ToList();
        }

        #region IRepository 

        public void Add(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
           
            _dbContext.Users.InsertOne(entity);                               
        }

        public void Delete(Guid entityId)
        {
            var user = _dbContext.Users.AsQueryable().SingleOrDefault(x => x.Id == entityId);

            if (user == null)
                throw new MissingEntityException($"User with such entityId is not found");

            _dbContext.Users.DeleteOne(x => x.Id == entityId);            
        }

        public IEnumerable<User> FindAll()
        {
            return _dbContext.Users.AsQueryable().ToList();
        }

        public User FindById(Guid id)
        {
            var user = _dbContext.Users.AsQueryable().SingleOrDefault(u => u.Id == id);

            if (user == null)
                throw new MissingEntityException($"User with Id={id} is not found");

            return user;
        }

        public void Save(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var modifiedUser = entity;

            var currentUser = _dbContext.Users.AsQueryable().SingleOrDefault(u => u.Id == modifiedUser.Id);

            if (currentUser == null)
                throw new MissingEntityException($"User with Id={entity.Id} is not found");

            currentUser.ChangeLogin(modifiedUser.Login);
            currentUser.ChangeEmail(modifiedUser.Email);
            currentUser.ChangePhone(modifiedUser.Phone);
            currentUser.ChangeFirstName(modifiedUser.FirstName);
            currentUser.ChangeLastName(modifiedUser.LastName);
            currentUser.ChangePassword(modifiedUser.PasswordHash);            
            currentUser.ChangePhoto(modifiedUser.Photo);

            if (modifiedUser.IsActive)
                currentUser.Activate();
            else
                currentUser.Deactivate();

            _dbContext.Users.ReplaceOne(x => x.Id == entity.Id, currentUser, new UpdateOptions(){IsUpsert = true});
        }

        #endregion
    }
}
