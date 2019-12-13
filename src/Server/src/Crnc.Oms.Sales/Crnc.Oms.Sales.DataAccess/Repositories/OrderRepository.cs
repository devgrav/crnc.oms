using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Sales.DataAccess.Repositories
{
    public class OrderRepository
        : IOrderRepository
    {
        private readonly SalesDataContext _dbContext;

        public OrderRepository(SalesDataContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IEnumerable<Order>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(x => x.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public void Add(Order entity)
        {
            _dbContext.Orders.Add(entity);
        }

        public void Delete(Order entity)
        {
            _dbContext.Orders.Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}