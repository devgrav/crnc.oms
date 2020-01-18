using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Dto;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.Security.Infrastructure.DataAccess.QueryExtensions;
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
        private readonly MongoDataContext _dbContext;

        public MongoDbUserRepository(MongoDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<User> FindByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(UserQueries.ByLogin(login), cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with such login is not found");

            return user;
        }

        public virtual async Task<IEnumerable<UserItemDto>> FindByFilterAsync(UserFilterDto dto,CancellationToken cancellationToken = default)
        {
            var users = await _dbContext.Users.AsQueryable().ToListAsync(cancellationToken);

            var usersQuery = users.AsQueryable();
            UserQueries.ByFilter(dto).ForEach(e => usersQuery = usersQuery.Where(e)); 
            
            var usersDto = usersQuery.Select(UserQueries.AsUserItemDtoProjection()).ToList();

            return usersDto;
        }

        public virtual async Task<IEnumerable<UserShortInfoItemDto>> FindByFilterShortInfoAsync(UserFilterDto dto, CancellationToken cancellationToken = default)
        {
            var users = await _dbContext.Users.AsQueryable().ToListAsync(cancellationToken);

            var usersQuery = users.AsQueryable();
            UserQueries.ByFilter(dto).ForEach(e => usersQuery = usersQuery.Where(e)); 
            
            var usersDto = usersQuery.Select(UserQueries.AsUserShortInfoItemDtoProjection()).ToList();

            return usersDto;
        }

        public virtual async Task<IEnumerable<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles.AsQueryable().ToListAsync(cancellationToken);
        }

        public virtual async Task<Role> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles.AsQueryable().SingleOrDefaultAsync(RoleQueries.ById(id), cancellationToken);
        }

        #region IRepository 

        public virtual async Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var isExisted = await _dbContext.Users.AsQueryable().AnyAsync(UserQueries.IsExisted(entity), cancellationToken);

            if(isExisted)
                throw new EntityAlreadyExistedException("User has already existed");
           
            await _dbContext.Users.InsertOneAsync(entity, cancellationToken: cancellationToken);                               
        }

        public virtual async Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(UserQueries.ById(entityId), cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with such entityId is not found");

            await _dbContext.Users.DeleteOneAsync(UserQueries.ById(entityId),cancellationToken);            
        }

        public virtual async Task<IEnumerable<User>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.AsQueryable().ToListAsync(cancellationToken);
        }

        public virtual async Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(UserQueries.ById(id),cancellationToken);

            if (user == null)
                throw new MissingEntityException($"User with Id={id} is not found");

            return user;
        }

        public virtual async Task SaveAsync(User entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var modifiedUser = entity;

            var currentUser = await _dbContext.Users.AsQueryable().SingleOrDefaultAsync(UserQueries.ById(modifiedUser.Id), cancellationToken);

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

            await _dbContext.Users.ReplaceOneAsync(UserQueries.ById(modifiedUser.Id), currentUser, new ReplaceOptions(){IsUpsert = true},cancellationToken);
        }

        #endregion
    }
}
