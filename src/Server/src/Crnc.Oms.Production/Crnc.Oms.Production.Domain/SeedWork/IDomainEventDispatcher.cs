using System.Threading.Tasks;

namespace Crnc.Oms.Production.Domain.SeedWork
{
    public interface IDomainEventDispatcher
    {
        void Dispatch(DomainEvent domainEvent);
        
        Task DispatchAsync(DomainEvent domainEvent);
    }
}