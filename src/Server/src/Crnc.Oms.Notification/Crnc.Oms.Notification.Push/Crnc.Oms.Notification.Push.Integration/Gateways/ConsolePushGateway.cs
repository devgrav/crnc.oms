using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Integration.Dto;
using Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Push.Integration.Gateways
{
    public class ConsolePushGateway
        : IPushGateway
    {
        private readonly ILogger<ConsolePushGateway> _logger;

        public ConsolePushGateway(ILogger<ConsolePushGateway> logger)
        {
            _logger = logger;
        }

        public Task<PushMessageOutputDto> SendPushAsync(PushMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Push sent in PushService with id {dto.MessageId}, receiverUserId : {dto.ReceiverUserId}, message: {dto.Message}");

            return Task.FromResult(new PushMessageOutputDto()
            {
                MessageId = dto.MessageId
            });
        }
    }
}