using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Domain.Aggregates.Order;


namespace Crnc.Oms.Sales.Domain.Repositories
{
    public interface IOrderRepository
        : IRepository<Order>
    {
        Task<Manager> GetManagerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}