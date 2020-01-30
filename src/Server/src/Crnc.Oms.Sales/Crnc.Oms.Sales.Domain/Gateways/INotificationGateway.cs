using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Domain.Gateways
{
    public interface INotificationGateway
    {
        Task NotifyManagerAsync(string notification, Manager manager, CancellationToken cancellationToken = default);
    }
}