using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Email.Application.Services
{
    public class EmailNotificationService
        : IEmailNotificationService
    {
        private readonly IEmailGateway _emailGateway;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IEmailGateway emailGateway, ILogger<EmailNotificationService> logger)
        {
            _emailGateway = emailGateway;
            _logger = logger;
        }
        
        public async Task<SendEmailMessageOutputDto> SendAsync(SendEmailMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
            
            var sentOutput = await _emailGateway.SendEmailAsync(new EmailMessageInputDto
                (dto.MessageId, dto.Sender, dto.Receiver, dto.Message), cancellationToken);
            
            return new SendEmailMessageOutputDto()
            {
                MessageId = sentOutput.MessageId
            };
        }
    }
}