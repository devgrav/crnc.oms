using System;
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