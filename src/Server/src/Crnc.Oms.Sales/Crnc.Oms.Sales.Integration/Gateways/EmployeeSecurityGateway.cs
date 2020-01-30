using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EmployeeSecurityGateway
        : IEmployeeGateway
    {
        private readonly ILogger<EmployeeSecurityGateway> _logger;
        private readonly RestClient _client;

        public EmployeeSecurityGateway(IOptions<IntegrationEndpointSettings> settings, ILogger<EmployeeSecurityGateway> logger,  ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _client = new RestClient(settings.Value.SecurityServiceEndpoint)
            {
                Authenticator = new JwtAuthenticator(currentUserContext.AuthToken)
            };
        }
        
        public async Task<List<Manager>> GetMainManagersAsync(CancellationToken cancellationToken = default)
        {
            var roles = new List<string>()
            {
                UserRoles.MainManager
            };
            
            var users = await GetUsersByRolesAsync(roles, cancellationToken);
            
            return users.Select(x =>
                new Manager(new FullName(
                    x.FirstName, 
                    x.LastName), 
                        new Email(x.Email), 
                        x.Login, 
                        x.Id))
                .ToList();
        }

        private async Task<List<UserItemDto>> GetUsersByRolesAsync(List<string> roles,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting users by roles {string.Join(",", roles)} from Security service");

            var request = new RestRequest("/api/users", DataFormat.Json)
            {
                Method = Method.GET
            };
            
            roles.ForEach(x => request.AddParameter("roles", x, ParameterType.GetOrPost));
            
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
            
            _logger.LogInformation($"Got users by roles {string.Join(",", roles)} from Sales service");

            return response.Data;
        }
    }
}