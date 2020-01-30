using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class OrderStatusChanged
        : DomainEvent
    {
        public OrderStatus OldStatus { get; set; }
        
        public OrderStatus NewStatus { get; set; }

        public DateTime NewStatusDate { get; set; }

        public Manager ChangedManager { get; set; }
        
        public OrderStatusChanged(OrderStatus oldStatus, OrderStatus newStatus, Manager changedManager, DateTime newStatusDate)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedManager = changedManager;
            NewStatusDate = newStatusDate;
        }
    }
}