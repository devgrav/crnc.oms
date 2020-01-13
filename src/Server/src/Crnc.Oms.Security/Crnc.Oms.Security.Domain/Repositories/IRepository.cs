using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Dto;
using Crnc.Oms.Security.Domain.SeedWork;

namespace Crnc.Oms.Security.Domain.Repositories
{
    /// <summary>
    /// Base interface of repository
    /// </summary>
    public interface IRepository<TEntity>
        where TEntity: IAggregateRoot
    {
        /// <summary>
        /// Find all aggregate root entities
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Find aggregate root entity by id
        /// </summary>
        /// <param name="id">Id of entity</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        Task<TEntity> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add aggregate root entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete aggregate root entity
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
