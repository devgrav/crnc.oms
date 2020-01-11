using System;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Notification.Push.Client.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Crnc.Oms.Notification.Push.Client.Push
{
    public class PushConnector
        : IPushConnector
    {
        private readonly AuthSettings _authSettings;
        private readonly ILogger<PushConnector> _logger;
        private readonly IAuthClient _authClient;
        private readonly IntegrationEndpointSettings _integrationSettings;
        private HubConnection _connection;
        
        public PushConnector(IOptions<IntegrationEndpointSettings> integrationSettings, 
            IOptions<AuthSettings> authSettings, ILogger<PushConnector> logger, IAuthClient authClient)
        {
            _authSettings = authSettings.Value;
            _logger = logger;
            _authClient = authClient;
            _integrationSettings = integrationSettings.Value;
            
            CreateConnection();
        }
        
        public async Task Connect()
        {
            if(_connection.State != HubConnectionState.Disconnected)
                return;
            
            _logger.LogInformation("Trying to connect to server...");
            await TryOpenConnection();
        }

        public async Task Disconnect()
        {
            await _connection.StopAsync();
        }
        
        private void CreateConnection()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{_integrationSettings.PushNotificationServiceEndpoint}/hubs/push",options =>
                { 
                    options.AccessTokenProvider = async () => (await _authClient.GetJwtTokenAsync(new AuthInfoDto()
                    {
                        Login = _authSettings.Login,
                        Password = _authSettings.Password
                    })).Jwt;
                })
                .ConfigureLogging(logging =>{
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            _connection.Closed += Connection_Closed;
            
            _connection.On<string, string>("ReceivePushMessageAsync", (user, message) =>
            {
                var newMessage = $"UserId: {user}, Message: {message}";
                _logger.LogInformation(newMessage);
            });
        }

        private async Task TryOpenConnection(){
            _logger.LogInformation("Starting connection...");
            await _connection.StartAsync();
            _logger.LogInformation("Connection is successful...");
        }
        
        
        private async Task Connection_Closed(Exception arg){
            _logger.LogInformation("Connection is closed");
            await _connection.StopAsync();
            _connection.Closed -= Connection_Closed;
            await Connect();
        }

        public void Dispose()
        {
            Disconnect().GetAwaiter().GetResult();
        }
    }
}