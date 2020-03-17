using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Application.Dto;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using Crnc.Oms.Notification.Push.Integration.Dto;
using Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Crnc.Oms.Notification.Push.Application.Services
{
    public class PushNotificationService
        : IPushNotificationService
    {
        private readonly IPushGateway _pushGateway;

        private static readonly Prometheus.Counter SentPushCount = Metrics
            .CreateCounter("notification_push_sent_total", "Number of sent push");
        
        
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
            
            SentPushCount.Inc();
            
            return new SendPushMessageOutputDto()
            {
                MessageId = sentOutput.MessageId
            };
        }
    }
}