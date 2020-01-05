using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Crnc.Oms.Notification.Push.Client
{
    class Program
    {
        private static HubConnection _connection;
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Building connection...");
            
            CreateConnection();
            
            Console.WriteLine("Built connection...");

            
            Console.WriteLine("Connect to hub...");
            await ConnectToHub();
            Console.WriteLine("Connection to hub started");

            Console.ReadLine();
        }

        private static void CreateConnection()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8107/hubs/push")
                .Build();

            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await _connection.StartAsync();
            };
        }

        private static async Task ConnectToHub()
        {
            _connection.On<string, string>("ReceivePushMessageAsync", (user, message) =>
            {
                
                    var newMessage = $"{user}: {message}";
                    Console.WriteLine(newMessage);
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                await _connection.StartAsync();
                Console.WriteLine(ex.Message);
            }
        }
    }
}