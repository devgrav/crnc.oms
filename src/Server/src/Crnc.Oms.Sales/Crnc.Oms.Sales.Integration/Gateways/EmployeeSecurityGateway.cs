using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Integration.Dto;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Integration.Gateways
{
    public class EmployeeSecurityGateway
        : IEmployeeGateway
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _client;
        private readonly ILogger<EmployeeSecurityGateway> _logger;
        private readonly ICurrentUserContext _currentUserContext;

        public EmployeeSecurityGateway(HttpClient client, ILogger<EmployeeSecurityGateway> logger, ICurrentUserContext currentUserContext)
        {
            _client = client;
            _logger = logger;
            _currentUserContext = currentUserContext;
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

            var query = string.Join("&", roles.Select(r => $"roles={Uri.EscapeDataString(r)}"));
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users?{query}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentUserContext.AuthToken);

            var response = await _client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error retrieving response from Security Api. Status code is {response.StatusCode}");
            }

            _logger.LogInformation($"Got users by roles {string.Join(",", roles)} from Security service");

            return await response.Content.ReadFromJsonAsync<List<UserItemDto>>(SerializerOptions, cancellationToken);
        }
    }
}
