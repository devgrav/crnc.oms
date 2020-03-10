using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Domain.Gateways
{
    public interface IEmployeeGateway
    {
        Task<List<Manager>> GetMainManagersAsync(CancellationToken cancellationToken = default);
    }
}