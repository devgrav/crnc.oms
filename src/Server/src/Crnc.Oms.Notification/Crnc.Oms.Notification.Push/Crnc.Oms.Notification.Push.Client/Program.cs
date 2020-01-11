using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Notification.Push.Client.Auth;
using Crnc.Oms.Notification.Push.Client.Push;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Polly;

namespace Crnc.Oms.Notification.Push.Client
{
    class Program
    {
        public static void Main(string[] args)
        { 
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

                    var config = builder.Build();

                    services.AddHostedService<PushConnectorWorker>();
                    services.AddSingleton<IPushConnector, PushConnector>();
                    services.AddSingleton<IAuthClient, AuthClient>();
                    
                    services.AddOptions();
                    services.Configure<AuthSettings>(config.GetSection("Auth"));
                    services.Configure<IntegrationEndpointSettings>(config.GetSection("IntegrationEndpoints"));
                });
    }
}