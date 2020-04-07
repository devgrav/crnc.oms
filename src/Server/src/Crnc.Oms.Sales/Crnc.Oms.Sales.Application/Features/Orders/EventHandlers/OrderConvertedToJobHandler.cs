using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Application.Features.Orders.Events
{
    public class OrderConvertedToJobHandler
        : INotificationHandler<OrderConvertedToJob>, IDomainEventNotificationHandler
    {
        private readonly IProductionJobGateway _productionJobGateway;
        private readonly ILogger<OrderConvertedToJobHandler> _logger;

        public OrderConvertedToJobHandler(IProductionJobGateway productionJobGateway,
            ILogger<OrderConvertedToJobHandler> logger)
        {
            _productionJobGateway = productionJobGateway;
            _logger = logger;
        }

        public async Task Handle(OrderConvertedToJob notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification;

            try
            {
                await _productionJobGateway.CreateJobAsync(domainEvent.JobInfo, domainEvent.Manager,
                    domainEvent.MaterialSource, domainEvent.OrderId, domainEvent.OrderNumber);
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not create job. Error details: {e}");
            }
        }
    }
}