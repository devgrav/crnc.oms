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


namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class EmailGateway
        : IEmailGateway
    {
        private readonly ILogger<EmailGateway> _logger;
        private readonly RestClient _client;

        public EmailGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<EmailGateway> logger)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.EmailNotificationServiceEndpoint);
        }

        public async Task<SendEmailOutputDto> SendEmailAsync(SendEmailInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email is sending with id {dto.MessageId}; sender : {dto.SenderEmail}; receiver: {dto.ReceiverEmail}");
            
            var request = new RestRequest("/api/emailNotifications", DataFormat.Json);
            request.AddJsonBody(dto);

            var response = await _client.PostAsync<SendEmailOutputDto>(request);

            _logger.LogInformation($"Email sent with id {dto.MessageId}; sender : {dto.SenderEmail} receiver: {dto.ReceiverEmail}");

            return response;
        }
    }
}