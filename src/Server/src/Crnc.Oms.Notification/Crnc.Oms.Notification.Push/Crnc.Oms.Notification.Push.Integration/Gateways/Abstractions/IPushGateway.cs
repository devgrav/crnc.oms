using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions
{
    public interface IPushGateway
    {
        Task<PushMessageOutputDto> SendPushAsync(PushMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}