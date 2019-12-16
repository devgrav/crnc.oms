using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways
{
    public class EmailGateway
        : IEmailGateway
    {
        private readonly ILogger<EmailGateway> _logger;

        public EmailGateway(ILogger<EmailGateway> logger)
        {
            _logger = logger;
        }

        public Task<SendEmailOutputDto> SendEmailAsync(SendEmailInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email sent with id {dto.MessageId}, sender : {dto.Sender}");

            return Task.FromResult(new SendEmailOutputDto()
            {
                MessageId = dto.MessageId
            });
        }
    }
}