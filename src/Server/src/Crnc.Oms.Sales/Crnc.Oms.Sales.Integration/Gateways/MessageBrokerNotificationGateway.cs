using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Integration.Dto;
using MassTransit;
using MassTransit.Conductor.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Sales.Integration.Gateways
{
    public class MessageBrokerNotificationGateway
        : INotificationGateway
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<MessageBrokerNotificationGateway> _logger;

        public MessageBrokerNotificationGateway(ISendEndpointProvider sendEndpointProvider, ILogger<MessageBrokerNotificationGateway> logger)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }
        
        public async Task NotifyManagerAsync(string notification, Manager manager, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Notification is sending from Sales service to message broker with id, user id : {manager.Id}, message: {notification}");

            await _sendEndpointProvider.Send<SendNotificationToUserCommand>(new SendNotificationToUserCommandDto()
            {
                Message = notification,
                UserId = manager.Id
            }, cancellationToken);
            
            _logger.LogInformation($"Notification sent from Sales service to message broker with id, user id : {manager.Id}, message: {notification}");
        }
    }
}