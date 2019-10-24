using System;
using System.Collections.Generic;

namespace Crnc.Oms.Domain.SeedWork
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
        IEnumerable<TEntity> FindAll();

        /// <summary>
        /// Find aggregate root entity by id
        /// </summary>
        /// <param name="id">Id of entity</param>
        /// <returns></returns>
        TEntity FindById(Guid id);

        /// <summary>
        /// Add aggregate root entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Add(TEntity entity);

        /// <summary>
        /// Delete aggregate root entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(Guid entityId);

        /// <summary>
        /// Save changes of entity
        /// </summary>
        void Save(TEntity entity);
    }
}
