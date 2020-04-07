using System;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Domain.Gateways
{
    public interface IProductionJobGateway
    {
        Task CreateJobAsync(JobInfo jobInfo, Manager manager, MaterialSource materialSource, Guid orderId, string orderNumber);
    }
}