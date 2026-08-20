using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Exceptions;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    /// <summary>
    /// Достаёт карточку пользователя из Security, чтобы узнать, куда доставлять уведомление.
    /// <para>
    /// Заголовок Authorization намеренно не отправляется: <c>GET /api/users/{id}</c> у Security
    /// помечен <c>[AllowAnonymous]</c>, и на этом держится разрешение канала доставки — контракт
    /// <c>SendNotificationToUserCommand</c> несёт только идентификатор пользователя, а куда
    /// доставлять, решает этот контекст. Подробности — в разделе «Разрешение канала доставки»
    /// файла docs/migrations/notification-net10-migration-plan.md.
    /// </para>
    /// </summary>
    public class UserInfoGateway
        : IUserInfoGateway
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _client;
        private readonly ILogger<UserInfoGateway> _logger;

        public UserInfoGateway(HttpClient client, ILogger<UserInfoGateway> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<GetUserInfoOutputDto> GetUserInfoAsync(GetUserInfoInputDto inputDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Getting user info from gateway with user's id {inputDto.UserId}");

            var response = await _client.GetAsync($"api/users/{inputDto.UserId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new MissingUserException();

                throw new Exception(
                    $"Error retrieving response from Security Api. Status code is {response.StatusCode}");
            }

            _logger.LogInformation($"Got user info from gateway with user's id {inputDto.UserId}");

            return await response.Content.ReadFromJsonAsync<GetUserInfoOutputDto>(
                SerializerOptions, cancellationToken);
        }
    }
}
