using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Events;
using Crnc.Oms.Production.Domain.Gateways;
using Crnc.Oms.Production.Integration.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Production.Integration.Gateways
{
    public class SalesOrderGateway
        : ISalesOrderGateway
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<SalesOrderGateway> _logger;

        public SalesOrderGateway(IPublishEndpoint publishEndpoint, ILogger<SalesOrderGateway> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        
        public async Task NotifyThatJobForOrderCreatedAsync(Guid jobId, string jobNumber, Guid orderId)
        {
            _logger.LogInformation($"JobForOrderCreated event is sending from gateway to broker with job id {jobId}");
            
            await _publishEndpoint.Publish<JobCreatedForOrderEvent>(new JobCreatedForOrderDto()
            {
                JobId = jobId,
                JobNumber = jobNumber,
                OrderId = orderId
            });
            
            _logger.LogInformation($"JobForOrderCreated sent from gateway to broker with job id {jobId}");
        }
    }
}