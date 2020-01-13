using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Push.Client.Push
{
    public class PushConnectorWorker
        : BackgroundService
    {
        private readonly ILogger<PushConnectorWorker> _logger;
        private readonly IPushConnector _pushConnector;

        public PushConnectorWorker(ILogger<PushConnectorWorker> logger, IPushConnector pushConnector)
        {
            _logger = logger;
            _pushConnector = pushConnector;
        }
        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Push worker is running.");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _pushConnector.Connect();
                }
                catch (Exception)
                { 
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Push worker is stopping..");

            await Task.CompletedTask;
        }
    }
}