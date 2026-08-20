using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Client.Settings;
using Crnc.Oms.Notification.Push.Client.Auth;
using Crnc.Oms.Notification.Push.Client.Push;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Crnc.Oms.Notification.Push.Client
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configBuilder.AddJsonFile("appsettings.json", optional: true);
                    configBuilder.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    configLogging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    services.AddHostedService<PushConnectorWorker>();
                    services.AddSingleton<IPushConnector, PushConnector>();
                    services.AddHttpClient<IAuthClient, AuthClient>();
                    
                    services.AddOptions();
                    services.Configure<AuthSettings>(config.GetSection("Auth"));
                    services.Configure<IntegrationEndpointSettings>(config.GetSection("IntegrationEndpoints"));
                });
    }
}