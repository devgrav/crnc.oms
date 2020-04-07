using System;
using System.Threading.Tasks;

namespace Crnc.Oms.Production.Domain.Gateways
{
    public interface ISalesOrderGateway
    {
        Task NotifyThatJobForOrderCreatedAsync(Guid jobId, string jobNumber, Guid orderId);
    }
}