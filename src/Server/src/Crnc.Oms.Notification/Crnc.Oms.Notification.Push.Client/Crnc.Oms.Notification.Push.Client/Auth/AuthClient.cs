using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Client.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crnc.Oms.Notification.Push.Client.Auth
{
    public class AuthClient
        : IAuthClient
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly ILogger<AuthClient> _logger;
        private readonly HttpClient _client;

        public AuthClient(IOptions<IntegrationEndpointSettings> settings, ILogger<AuthClient> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
            _client.BaseAddress = new Uri(settings.Value.SecurityServiceEndpoint.TrimEnd('/') + "/");
        }

        public async Task<AuthUserDto> GetJwtTokenAsync(AuthInfoDto authInfo, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting auth info with user's login {authInfo.Login}");

            var response = await _client.PostAsJsonAsync("api/accounts/auth", authInfo, SerializerOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception("User is not found");

                throw new Exception(
                    $"Error retrieving response from Security Api. Status code is {response.StatusCode}");
            }

            _logger.LogInformation($"Got auth info from security api with user's login {authInfo.Login}");

            return await response.Content.ReadFromJsonAsync<AuthUserDto>(SerializerOptions, cancellationToken);
        }
    }
}
