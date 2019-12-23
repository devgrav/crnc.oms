using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Repositories
{
    /// <summary>
    /// Repository for users
    /// </summary>
    public class MongoDbUserRepository
        : IUserRepository
    {
        private MongoDataContext _dbContext;

        public MongoDbUserRepository(MongoDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> FindByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(u => u.Login == login, cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with such login is not found");

            return user;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles.AsQueryable().ToListAsync(cancellationToken);
        }

        public async Task<Role> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles.AsQueryable().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        #region IRepository 

        public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var isExisted = await _dbContext.Users.AsQueryable().AnyAsync(x => x.Id == entity.Id 
                                                                               || x.Login.ToLower() == entity.Login.ToLower(), cancellationToken);

            if(isExisted)
                throw new EntityAlreadyExistedException("User has already existed");
           
            await _dbContext.Users.InsertOneAsync(entity);                               
        }

        public async Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(x => x.Id == entityId, cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with such entityId is not found");

            await _dbContext.Users.DeleteOneAsync(x => x.Id == entityId,cancellationToken);            
        }

        public async Task<IEnumerable<User>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.AsQueryable().ToListAsync(cancellationToken);
        }

        public async Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(u => u.Id == id,cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with Id={id} is not found");

            return user;
        }

        public async Task SaveAsync(User entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var modifiedUser = entity;

            var currentUser = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(u => u.Id == modifiedUser.Id, cancellationToken);

            if (currentUser == null)
                throw new MissingEntityException($"User with Id={entity.Id} is not found");

            currentUser.ChangeLogin(modifiedUser.Login);
            currentUser.ChangeEmail(modifiedUser.Email);
            currentUser.ChangePhone(modifiedUser.Phone);
            currentUser.ChangeFirstName(modifiedUser.FirstName);
            currentUser.ChangeLastName(modifiedUser.LastName);
            currentUser.ChangePassword(modifiedUser.PasswordHash, modifiedUser.PasswordSalt);            
            currentUser.ChangePhoto(modifiedUser.Photo);
            currentUser.ChangeRole(modifiedUser.Role);

            if (modifiedUser.IsActive)
                currentUser.Activate();
            else
                currentUser.Deactivate();

            await _dbContext.Users.ReplaceOneAsync(x => x.Id == entity.Id, currentUser, new UpdateOptions(){IsUpsert = true},cancellationToken);
        }

        #endregion
    }
}
