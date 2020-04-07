using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Production.DataAccess.Repositories
{
    public abstract class Repository<TEntity>
        : IRepository<TEntity>
        where TEntity : DomainEntity, IAggregateRoot
    {
        protected readonly ProductionDataContext _dataContext;

        public Repository(ProductionDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dataContext.Set<TEntity>().ToListAsync(cancellationToken);
        }

        public async Task<TEntity> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dataContext.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
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
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}