using Crnc.Oms.Sales.Domain.SeedWork;
using MediatR;

namespace Crnc.Oms.Sales.Application
{
    public class DomainEventNotification<TDomainEvent> 
        : INotification 
        where TDomainEvent: DomainEvent
    {
        public TDomainEvent DomainEvent { get; }

        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}