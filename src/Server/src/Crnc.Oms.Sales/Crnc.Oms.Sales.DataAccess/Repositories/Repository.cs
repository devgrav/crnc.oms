using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Sales.DataAccess.Repositories
{
    public abstract class Repository<TEntity>
        : IRepository<TEntity>
        where TEntity: DomainEntity, IAggregateRoot
    {
        protected readonly SalesDataContext _dataContext;
        protected readonly IDomainEventDispatcher _domainEventDispatcher;

        public Repository(SalesDataContext dataContext, IDomainEventDispatcher domainEventDispatcher)
        {
            _dataContext = dataContext;
            _domainEventDispatcher = domainEventDispatcher;
        }
        
        public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dataContext.Set<TEntity>().ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dataContext.Set<TEntity>().SingleOrDefaultAsync(x=> x.Id == id,cancellationToken);
        }

        public void Add(TEntity entity)
        {
            _dataContext.Set<TEntity>().Add(entity);
        }

        public void Delete(TEntity entity)
        {
            _dataContext.Set<TEntity>().Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync();
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchDomainEventsAsync()
        {
            var domainEventEntities = _dataContext.ChangeTracker.Entries<DomainEntity>()
                .Select(po => po.Entity)
                .Where(po => po.DomainEvents.Any())
                .ToArray();

            foreach (var entity in domainEventEntities)
            {
                var events = entity.DomainEvents.ToArray();
                foreach (var domainEvent in events)
                {
                    await _domainEventDispatcher.DispatchAsync(domainEvent);
                }
            }
        }

    }
}