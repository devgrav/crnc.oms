using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class OrderStatusChanged
        : DomainEvent
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public OrderStatus OldStatus { get; set; }
        
        public OrderStatus NewStatus { get; set; }

        public DateTime NewStatusDate { get; set; }

        public Manager ChangedManager { get; set; }
        
        public OrderStatusChanged(Guid orderId, string orderNumber, OrderStatus oldStatus, OrderStatus newStatus, Manager changedManager, DateTime newStatusDate)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedManager = changedManager;
            NewStatusDate = newStatusDate;
        }
    }
}