using System;
using System.Collections.Generic;
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
    public class UserGateway
        : IUserGateway
    {
        private readonly ILogger<UserGateway> _logger;
        private readonly RestClient _client;

        public UserGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<UserGateway> logger,  ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.SecurityServiceEndpoint)
            {
                Authenticator = new JwtAuthenticator(currentUserContext.AuthToken)
            };
        }
        
        public async Task<UsersByRolesOutputDto> GetUsersByRolesAsync(UsersByRolesInputDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting users by roles {string.Join(",", dto.Roles)} from Security service");

            var request = new RestRequest("/api/users", DataFormat.Json)
            {
                Method = Method.GET
            };
            dto.Roles.ForEach(x => request.AddParameter("roles", x, ParameterType.GetOrPost));
            
            var response = await _client.ExecuteAsync<List<UserItemDto>>(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                var  message = $"Error retrieving response from Sales Api. Status code is {response.StatusCode}";
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
            
            _logger.LogInformation($"Got users by roles {string.Join(",", dto.Roles)} from Sales service");

            return new UsersByRolesOutputDto()
            {
                Items = response.Data
            };
        }
    }
}