using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions
{
    public interface IEmailGateway
    {
        Task<EmailMessageOutputDto> SendEmailAsync(EmailMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}