using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class MessageBrokerEmailGateway
        : IEmailGateway
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<MessageBrokerEmailGateway> _logger;

        public MessageBrokerEmailGateway(ISendEndpointProvider sendEndpointProvider, ILogger<MessageBrokerEmailGateway> logger)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }


        public async Task<SendEmailOutputDto> SendEmailAsync(SendEmailInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email is sending from gateway to broker with id {dto.MessageId}; senderEmail : {dto.SenderEmail}; receiverEmail: {dto.ReceiverEmail}");
            
            await _sendEndpointProvider.Send<SendEmailNotificationToReceiverCommand>(dto, cancellationToken);
            
            _logger.LogInformation($"Email sent from gateway to broker with id {dto.MessageId}; senderEmail : {dto.SenderEmail}; receiverEmail: {dto.ReceiverEmail}");

            return new SendEmailOutputDto()
            {
                MessageId = dto.MessageId
            };
        }
    }
}