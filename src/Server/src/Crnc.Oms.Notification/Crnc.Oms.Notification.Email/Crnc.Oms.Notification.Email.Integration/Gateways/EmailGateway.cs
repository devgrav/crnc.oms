using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class EmailGateway
        : IEmailGateway
    {
        private readonly ILogger<EmailGateway> _logger;

        public EmailGateway(ILogger<EmailGateway> logger)
        {
            _logger = logger;
        }

        public Task<EmailMessageOutputDto> SendEmailAsync(EmailMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email sent in EmailService with id {dto.MessageId}, sender : {dto.Sender} to receiver {dto.Receiver}, message: {dto.Message}");

            return Task.FromResult(new EmailMessageOutputDto()
            {
                MessageId = dto.MessageId
            });
        }
    }
}