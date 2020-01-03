using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;


namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class EmailGateway
        : IEmailGateway
    {
        private readonly ILogger<EmailGateway> _logger;
        private readonly RestClient _client;

        public EmailGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<EmailGateway> logger, ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.EmailNotificationServiceEndpoint);
            _client.Authenticator = new JwtAuthenticator(currentUserContext.AuthToken);
        }

        public async Task<SendEmailOutputDto> SendEmailAsync(SendEmailInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email is sending from gateway with id {dto.MessageId}; senderEmail : {dto.SenderEmail}; receiverEmail: {dto.ReceiverEmail}");

            var request = new RestRequest("/api/emailNotifications", DataFormat.Json)
            {
                Method = Method.POST
            };
            request.AddJsonBody(dto);

            var response = await _client.ExecuteTaskAsync<SendEmailOutputDto>(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                var  message = $"Error retrieving response from Email notification api. Status code is {response.StatusCode}";
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

            _logger.LogInformation($"Email sent from gateway with id {dto.MessageId}; senderEmail : {dto.SenderEmail}; receiverEmail: {dto.ReceiverEmail}");

            return response.Data;
        }
    }
}