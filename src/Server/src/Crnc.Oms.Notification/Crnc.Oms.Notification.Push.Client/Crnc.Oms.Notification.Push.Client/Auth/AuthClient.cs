using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Notification.Push.Client.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Notification.Push.Client.Auth
{
    public class AuthClient
        : IAuthClient
    {
        private readonly ILogger<AuthClient> _logger;
        private readonly RestClient _client;

        public AuthClient(IOptions<IntegrationEndpointSettings> settings, ILogger<AuthClient> logger)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.SecurityServiceEndpoint);
        }
        
        public async Task<AuthUserDto> GetJwtTokenAsync(AuthInfoDto authInfo, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting auth info with user's login {authInfo.Login}");

            var request = new RestRequest("/api/accounts/auth")
            {
                Method = Method.POST,
            };
            request.AddJsonBody(authInfo);

            var response = await _client.ExecuteTaskAsync<AuthUserDto>(request, cancellationToken);

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
                    exception = new Exception("User is not found");
                }
                else
                {
                    exception = new Exception(message); 
                }
                
                throw exception;
            }
            
            _logger.LogInformation($"Got auth info from security api with user's login {authInfo.Login}");

            return response.Data;
        }
    }
}