using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Crnc.Oms.Notification.Push.Application.Dto;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Push.Application.Services
{
    public class PushNotificationService
        : IPushNotificationService
    {
        private readonly IPushGateway _pushGateway;

        public PushNotificationService(IPushGateway pushGateway)
        {
            _pushGateway = pushGateway;
        }
        
        public async Task<SendPushMessageOutputDto> SendAsync(SendPushMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
            
            var sentOutput = await _pushGateway.SendPushAsync(new PushMessageInputDto
                (dto.MessageId, dto.ReceiverUserId, dto.Message), cancellationToken);
            
            return new SendPushMessageOutputDto()
            {
                MessageId = sentOutput.MessageId
            };
        }
    }
}