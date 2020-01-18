using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Dto;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.Security.Infrastructure.DataAccess.QueryExtensions;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Repositories
{
    public class CachedMongoDbUserRepository
        : MongoDbUserRepository
    {
        private const string UsersCacheKey = "users";
        private const string RolesCacheKey = "roles";
        
        private readonly IEntityCollectionCacheProvider<User> _usersCollectionCache;
        private readonly IEntityCollectionCacheProvider<Role> _rolesCollectionCache;

        public CachedMongoDbUserRepository(MongoDataContext mongoDataContext, 
            IEntityCollectionCacheProvider<User> usersCollectionCache, IEntityCollectionCacheProvider<Role> rolesCollectionCache)
        : base(mongoDataContext)
        {
            _usersCollectionCache = usersCollectionCache;
            _rolesCollectionCache = rolesCollectionCache;
        }
        
        public override async Task<IEnumerable<User>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _usersCollectionCache.GetAsync(UsersCacheKey);

            return users;
        }

        public override async Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var users = await _usersCollectionCache.GetAsync(UsersCacheKey);

            return users.AsQueryable().Where(UserQueries.ById(id)).SingleOrDefault();
        }
        
        public override async Task<User> FindByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            var users = await _usersCollectionCache.GetAsync(UsersCacheKey);

            var user = users.AsQueryable().Where(UserQueries.ByLogin(login)).SingleOrDefault();
            if (user == null)
                throw new MissingEntityException($"User with such login is not found");
            
            return user;
        }

        public override async Task<IEnumerable<UserItemDto>> FindByFilterAsync(UserFilterDto dto, CancellationToken cancellationToken = default)
        {
            var users = await _usersCollectionCache.GetAsync(UsersCacheKey);

            var usersQuery = users.AsQueryable();
            UserQueries.ByFilter(dto).ForEach(e => usersQuery = usersQuery.Where(e)); 
            
            var usersDto = usersQuery.Select(UserQueries.AsUserItemDtoProjection()).ToList();

            return usersDto;
        }

        public override async Task<IEnumerable<UserShortInfoItemDto>> FindByFilterShortInfoAsync(UserFilterDto dto, CancellationToken cancellationToken = default)
        {
            var users = await _usersCollectionCache.GetAsync(UsersCacheKey);

            var usersQuery = users.AsQueryable();
            UserQueries.ByFilter(dto).ForEach(e => usersQuery = usersQuery.Where(e)); 
            
            var usersDto = usersQuery.Select(UserQueries.AsUserShortInfoItemDtoProjection()).ToList();

            return usersDto;
        }

        public override async Task<IEnumerable<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _rolesCollectionCache.GetAsync(RolesCacheKey);
            
            return roles.ToList();
        }

        public override async Task<Role> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var roles = await _rolesCollectionCache.GetAsync(RolesCacheKey);

            return roles.AsQueryable().SingleOrDefault(RoleQueries.ById(id));
        }

        public override async Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            await base.AddAsync(entity, cancellationToken);
            await _usersCollectionCache.RefreshAsync(UsersCacheKey);
        }

        public override async Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            await base.DeleteAsync(entityId, cancellationToken);
            await _usersCollectionCache.RefreshAsync(UsersCacheKey);
        }

        public override async Task SaveAsync(User entity, CancellationToken cancellationToken = default)
        {
            await base.SaveAsync(entity, cancellationToken);
            await _usersCollectionCache.RefreshAsync(UsersCacheKey);
        }
    }
}