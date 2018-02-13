using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.DataAccess.Exceptions;

namespace Crnc.Oms.DataAccess.Repositories
{
    /// <summary>
    /// Repository for users
    /// </summary>
    public class UserRepository
        : IUserRepository
    {
        private DataContext _dbContext;

        public UserRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User FindByLogin(string login)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Login == login);

            if (user == null)
                throw new MissingEntityException($"User with such login is not found");

            return user;
        }

        public IEnumerable<Role> GetRoles()
        {
            return _dbContext.Roles.ToList();
        }

        #region IRepository 

        public void Add(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
           
            _dbContext.Users.Add(entity);

            _dbContext.SaveChanges();                       
        }

        public void Delete(int entityId)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Id == entityId);

            if (user == null)
                throw new MissingEntityException($"User with Id={entityId} is not found");

            user.Deactivate();

            _dbContext.SaveChanges();
        }

        public IEnumerable<User> FindAll()
        {
            return _dbContext.Users.Include(u => u.Role).AsNoTracking().ToList();
        }

        public User FindById(int id)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Id == id);

            if (user == null)
                throw new MissingEntityException($"User with Id={id} is not found");

            return user;
        }

        public void Save(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var modifiedUser = entity;

            var currentUser = _dbContext.Users.SingleOrDefault(u => u.Id == modifiedUser.Id);

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

            _dbContext.SaveChanges();
        }

        #endregion
    }
}
