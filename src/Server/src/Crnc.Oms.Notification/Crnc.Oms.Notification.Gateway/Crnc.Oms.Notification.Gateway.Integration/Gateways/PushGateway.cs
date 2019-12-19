using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class PushGateway
        : IPushGateway
    {
        private readonly ILogger<EmailGateway> _logger;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly RestClient _client;

        public PushGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<EmailGateway> logger, ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _currentUserContext = currentUserContext;
            _client = new RestClient(settings.Value.PushNotificationServiceEndpoint);
        }


        public async Task<SendPushOutputDto> SendPushAsync(SendPushInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Push is sending from gateway with id {dto.MessageId}, receiver id : {dto.ReceiverUserId}");
            
            var request = new RestRequest("/api/pushNotifications", DataFormat.Json);
            request.Method = Method.POST;
            request.AddJsonBody(dto);
            
            var response = await _client.ExecuteTaskAsync<SendPushOutputDto>(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                var  message = $"Error retrieving response from Push Notification Api. Status code is {response.StatusCode}";
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
            
            _logger.LogInformation($"Push sent from gateway with id {dto.MessageId}, receiver id : {dto.ReceiverUserId}");

            return response.Data;
        }
    }
}