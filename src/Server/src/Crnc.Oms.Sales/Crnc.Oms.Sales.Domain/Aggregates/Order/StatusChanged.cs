using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class StatusChanged
        : DomainEvent
    {
        public OrderStatus OldStatus { get; set; }
        
        public OrderStatus NewStatus { get; set; }
        
        public StatusChanged(OrderStatus oldStatus, OrderStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}