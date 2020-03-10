using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Exceptions;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class UserInfoGateway
        : IUserInfoGateway
    {
        private readonly ILogger<UserInfoGateway> _logger;
        private readonly RestClient _client;

        public UserInfoGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<UserInfoGateway> logger)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.SecurityServiceEndpoint);
        }
        
        public async Task<GetUserInfoOutputDto> GetUserInfoAsync(GetUserInfoInputDto inputDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting user info from gateway with user's id {inputDto.UserId}");

            var request = new RestRequest("/api/users/{id}")
            {
                Method = Method.GET
            };
            request.AddUrlSegment("id", inputDto.UserId);

            var response = await _client.ExecuteTaskAsync<GetUserInfoOutputDto>(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                var  message = $"Error retrieving response from Security Api. Status code is {response.StatusCode}";
                Exception exception;
                if (response.ErrorException != null)
                {
                    message = $"{message}. Details in inner exception";
                    exception = new Exception(message, response.ErrorException);
                }
                else if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    exception = new MissingUserException();
                }
                else
                {
                    exception = new Exception(message); 
                }
                
                throw exception;
            }
            
            _logger.LogInformation($"Got user info from gateway with user's id {inputDto.UserId}");

            return response.Data;
        }
    }
}