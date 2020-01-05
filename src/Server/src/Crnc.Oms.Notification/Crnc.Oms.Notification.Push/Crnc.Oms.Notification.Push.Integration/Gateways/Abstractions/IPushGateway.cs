using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Dto;

namespace Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions
{
    public interface IPushGateway
    {
        Task<PushMessageOutputDto> SendPushAsync(PushMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}