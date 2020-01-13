using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Sales.Domain.Dto;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.SeedWork;
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


        public async Task<NotifyUserOutputDto> NotifyUserAsync(NotifyUserInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Notification is sending from Sales service with id, user id : {dto.UserId}, message: {dto.Message}");

            var request = new RestRequest("/api/notifications/user", DataFormat.Json)
            {
                Method = Method.POST
            };
            request.AddJsonBody(dto);
            
            var response = await _client.ExecuteAsync<NotifyUserOutputDto>(request, cancellationToken);

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
            
            _logger.LogInformation($"Notification sent from Sales service with id, user id : {dto.UserId}, message: {dto.Message}");

            return response.Data;
        }
    }
}