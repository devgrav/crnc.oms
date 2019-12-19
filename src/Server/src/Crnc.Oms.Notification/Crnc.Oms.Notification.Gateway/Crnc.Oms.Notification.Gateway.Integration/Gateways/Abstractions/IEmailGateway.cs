using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions
{
    public interface IEmailGateway
    {
        Task<SendEmailOutputDto> SendEmailAsync(SendEmailInputDto dto, CancellationToken cancellationToken = default);
    }
}