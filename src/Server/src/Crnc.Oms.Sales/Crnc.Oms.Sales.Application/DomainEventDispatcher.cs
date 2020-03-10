using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.SeedWork;
using MediatR;

namespace Crnc.Oms.Sales.Application
{
    public class DomainEventDispatcher
        : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;

        public DomainEventDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public void Dispatch(DomainEvent domainEvent)
        {
            _mediator.Publish(domainEvent).GetAwaiter().GetResult();
        }

        public async Task DispatchAsync(DomainEvent domainEvent)
        {
             await _mediator.Publish(domainEvent);
        }
    }
}