using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Email.Integration.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Crnc.Oms.Notification.Email.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Crnc.Oms.Notification.Email.Application.Services
{
    public class EmailNotificationService
        : IEmailNotificationService
    {
        private readonly IEmailGateway _emailGateway;
        public static readonly Prometheus.Counter SentEmailCount = Metrics
            .CreateCounter("notification_email_sent_total", "Number of sent email");

        public EmailNotificationService(IEmailGateway emailGateway)
        {
            _emailGateway = emailGateway;
        }
        
        public async Task<SendEmailMessageOutputDto> SendAsync(SendEmailMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
            
            var sentOutput = await _emailGateway.SendEmailAsync(new EmailMessageInputDto
                (dto.MessageId, dto.SenderEmail, dto.ReceiverEmail, dto.Message), cancellationToken);
            
            SentEmailCount.Inc();
            
            return new SendEmailMessageOutputDto()
            {
                MessageId = sentOutput.MessageId
            };
        }
    }
}