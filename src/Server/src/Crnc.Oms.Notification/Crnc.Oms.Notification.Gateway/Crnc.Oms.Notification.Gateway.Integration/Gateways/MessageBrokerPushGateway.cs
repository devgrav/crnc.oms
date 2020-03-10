using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class MessageBrokerPushGateway
        : IPushGateway
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<MessageBrokerPushGateway> _logger;

        public MessageBrokerPushGateway(ISendEndpointProvider sendEndpointProvider, ILogger<MessageBrokerPushGateway> logger)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }


        public async Task<SendPushOutputDto> SendPushAsync(SendPushInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Push is sending from gateway to broker with id {dto.MessageId}, receiver id : {dto.ReceiverUserId}");
            
            await _sendEndpointProvider.Send<SendPushNotificationToReceiverCommand>(dto, cancellationToken);
            
            _logger.LogInformation($"Push sent from gateway to broker with id {dto.MessageId}, receiver id : {dto.ReceiverUserId}");

            return new SendPushOutputDto()
            {
                MessageId = dto.MessageId
            };
        }
    }
}