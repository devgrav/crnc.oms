using Crnc.Oms.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.IRepositories
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
