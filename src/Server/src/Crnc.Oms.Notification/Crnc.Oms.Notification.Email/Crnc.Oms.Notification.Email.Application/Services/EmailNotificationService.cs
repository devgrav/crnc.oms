using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Crnc.Oms.Notification.Email.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Email.Application.Services
{
    public class EmailNotificationService
        : IEmailNotificationService
    {
        private readonly IEmailGateway _emailGateway;

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
            
            return new SendEmailMessageOutputDto()
            {
                MessageId = sentOutput.MessageId
            };
        }
    }
}