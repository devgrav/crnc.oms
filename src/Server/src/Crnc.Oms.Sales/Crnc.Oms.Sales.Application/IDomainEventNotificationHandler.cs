using MediatR;

namespace Crnc.Oms.Sales.Application
{
    public interface IDomainEventNotificationHandler<T>
        : INotificationHandler<T>
        where T: INotification
    {
        
    }
}