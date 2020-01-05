using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Polly;

namespace Crnc.Oms.Notification.Push.Client
{
    class Program
    {
        private static HubConnection _connection;

        private static string MyAccessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMmE4OTk4NWYtZjAxMy00ZjJhLTk1NDUtMzk1ZWZiNDNhMTQyIiwiZXhwIjoxNTc4MjcyNzgxLCJpc3MiOiJPbXNDcm5jQXV0aFNlcnZlciIsImF1ZCI6Ik9tc0NybmNBcGlzIn0.MSFwSRMqDG2BTKZWcyY304C2NtFfVfvF5yQRkddRo_Y";
        
        static void Main(string[] args)
        {
            CreateConnection();
            
            OpenConnection();
                
            Console.ReadLine();
        }

        private static void CreateConnection()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8107/hubs/push",options =>
                { 
                    options.AccessTokenProvider = () => Task.FromResult(MyAccessToken);
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
                Console.WriteLine(newMessage);
            });
        }

        private static async void OpenConnection(){
            var pauseBetweenFailures = TimeSpan.FromSeconds(20);
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => pauseBetweenFailures
                    , (exception, timeSpan) => {
                        Console.WriteLine(exception.ToString());
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Trying to connect to server...");
                await TryOpenConnection();
            });
        }
        
        private static async Task<bool> TryOpenConnection(){
            Console.WriteLine("Starting connection...");
            await _connection.StartAsync();
            Console.WriteLine("Connection is successful...");
            return true;
        }
        
        
        private static async Task Connection_Closed(Exception arg){
            Console.WriteLine("Connection is closed");
            await _connection.StopAsync();
            _connection.Closed -= Connection_Closed;
            OpenConnection();
        }
    }
}