using System;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Events;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Integration.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Integration.Gateways
{
    public class ProductionJobGateway
        : IProductionJobGateway
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProductionJobGateway> _logger;

        public ProductionJobGateway(IPublishEndpoint publishEndpoint, ILogger<ProductionJobGateway> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        public async Task CreateJobAsync(JobInfo jobInfo, Manager manager, MaterialSource materialSource, Guid orderId, string orderNumber)
        {
            _logger.LogInformation($"OrderConvertedToJobEvent is sending from Sales service to message broker for order {orderId}");

            await _publishEndpoint.Publish<OrderConvertedToJobEvent>(new OrderConvertedToJobEventDto()
            {
                JobDescription = jobInfo.Description,
                JobType = jobInfo.Type.ToString(),
                ManagerLogin = manager.Login,
                ManagerFullName = manager.FullName.Value,
                MaterialSource = materialSource.ToString(),
                OrderId = orderId,
                OrderNumber = orderNumber
            });
            
            _logger.LogInformation($"OrderConvertedToJobEvent sent from Sales service to message broker for order {orderId}");
        }
    }
}