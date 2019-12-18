using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
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
            _logger.LogInformation($"Push sent from gateway with id {dto.MessageId}, receiver id : {dto.ReceiverUserId}");

            return Task.FromResult(new SendPushOutputDto()
            {
                MessageId = dto.MessageId
            });
        }
    }
}