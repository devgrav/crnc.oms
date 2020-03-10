using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Repositories
{
    /// <summary>
    /// Base interface of repository
    /// </summary>
    public interface IRepository<TEntity>
        where TEntity: class, IAggregateRoot
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
        /// <returns></returns>
        Task<TEntity> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add aggregate root entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Add(TEntity entity);

        /// <summary>
        /// Delete aggregate root entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Save changes of entities
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
