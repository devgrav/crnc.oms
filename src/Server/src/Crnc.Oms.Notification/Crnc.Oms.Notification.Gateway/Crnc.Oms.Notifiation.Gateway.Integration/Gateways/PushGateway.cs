using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways
{
    public class PushGateway
        : IPushGateway
    {
        private readonly ILogger<PushGateway> _logger;

        public PushGateway(ILogger<PushGateway> logger)
        {
            _logger = logger;
        }
        
        public Task<SendPushOutputDto> SendPushAsync(SendPushInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Push sent with id {dto.MessageId}, sender : {dto.Sender}");

            return Task.FromResult(new SendPushOutputDto()
            {
                MessageId = dto.MessageId
            });
        }
    }
}