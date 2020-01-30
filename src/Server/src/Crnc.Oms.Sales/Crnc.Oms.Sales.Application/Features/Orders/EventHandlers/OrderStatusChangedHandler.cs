using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Gateways;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Application.Features.Orders.Events
{
    public class OrderStatusChangedHandler
        : IDomainEventNotificationHandler<DomainEventNotification<OrderStatusChanged>>
    {
        private readonly INotificationGateway _notificationGateway;
        private readonly IEmployeeGateway _employeeGateway;
        private readonly ILogger<OrderStatusChangedHandler> _logger;

        public OrderStatusChangedHandler(INotificationGateway notificationGateway, IEmployeeGateway employeeGateway, ILogger<OrderStatusChangedHandler> logger)
        {
            _notificationGateway = notificationGateway;
            _employeeGateway = employeeGateway;
            _logger = logger;
        }
        
        public async Task Handle(DomainEventNotification<OrderStatusChanged> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;
            var changedManager = domainEvent.ChangedManager;
            
            List<Manager> mainManagers = null;
            try
            {
                mainManagers = await _employeeGateway.GetMainManagersAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not notify main managers, because occured error from employee data gateway. Error details: {e}");
                return;
            }

            if (mainManagers != null && mainManagers.Any())
            {
                List<Task> notificationTasks = new List<Task>();
                foreach (var mainManager in mainManagers)
                {
                    var message = $"Status of order {EnumHelper.GetDescription(domainEvent.OldStatus)} changed to " +
                                  $"{EnumHelper.GetDescription(domainEvent.NewStatus)} at {domainEvent.NewStatusDate.ToStandartFormatWithTime()} by {changedManager.FullName} ({changedManager.Login})";
                    
                    notificationTasks.Add(_notificationGateway.NotifyManagerAsync(message, mainManager, cancellationToken));
                }

                await Task.WhenAll(notificationTasks);
            }
        }
    }
}