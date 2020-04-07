using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Sales.DataAccess.Repositories
{
    public class OrderRepository
        : Repository<Order>, IOrderRepository
    {
        public OrderRepository(SalesDataContext dbContext, IDomainEventDispatcher domainEventDispatcher)
            :base(dbContext, domainEventDispatcher)
        {
        }

        public override async Task<Order> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return
                await _dataContext.Orders.Include(x => x.Manager)
                    .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Manager> GetManagerByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return
                await _dataContext.Managers
                    .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}