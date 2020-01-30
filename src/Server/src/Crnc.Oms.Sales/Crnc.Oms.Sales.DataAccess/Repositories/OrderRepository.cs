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
        
    }
}