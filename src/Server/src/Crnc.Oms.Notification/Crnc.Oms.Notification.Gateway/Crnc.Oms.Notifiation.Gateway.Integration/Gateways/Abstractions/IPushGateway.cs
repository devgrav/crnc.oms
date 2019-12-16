using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions
{
    public interface IPushGateway
    {
        Task<SendPushOutputDto> SendPushAsync(SendPushInputDto dto, CancellationToken cancellationToken = default);
    }
}