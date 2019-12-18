using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions
{
    public interface IEmailGateway
    {
        Task<EmailMessageOutputDto> SendEmailAsync(EmailMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}