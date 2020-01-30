using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Integration.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Sales.Integration.Gateways
{
    public class NotificationGateway
        : INotificationGateway
    {
        private readonly ILogger<NotificationGateway> _logger;
        private readonly RestClient _client;

        public NotificationGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<NotificationGateway> logger,  ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.NotificationServiceEndpoint)
            {
                Authenticator = new JwtAuthenticator(currentUserContext.AuthToken)
            };
        }


        public async Task NotifyManagerAsync(string notification, Manager manager, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Notification is sending from Sales service with id, user id : {manager.Id}, message: {notification}");

            var request = new RestRequest("/api/notifications/user", DataFormat.Json)
            {
                Method = Method.POST
            };
            request.AddJsonBody(new NotificationUserDto()
            {
                Message = notification,
                UserId = manager.Id
            });
            
            var response = await _client.ExecuteAsync<string>(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                var  message = $"Error retrieving response from Notification Gateway Api. Status code is {response.StatusCode}";
                Exception exception;
                if (response.ErrorException != null)
                {
                    message = $"{message}. Details in inner exception";
                    exception = new Exception(message, response.ErrorException);
                }
                else
                {
                    exception = new Exception(message); 
                }
                
                throw exception;
            }
            
            _logger.LogInformation($"Notification sent from Sales service with id, user id : {manager.Id}, message: {notification}");
        }
    }
}