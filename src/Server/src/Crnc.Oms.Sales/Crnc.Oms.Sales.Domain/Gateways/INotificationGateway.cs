using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Dto;

namespace Crnc.Oms.Sales.Domain.Gateways
{
    public interface INotificationGateway
    {
        Task<NotifyUserOutputDto> NotifyUserAsync(NotifyUserInputDto dto, CancellationToken cancellationToken = default);
    }
}