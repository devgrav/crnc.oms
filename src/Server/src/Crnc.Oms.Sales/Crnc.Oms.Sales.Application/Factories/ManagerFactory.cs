using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Application.Factories
{
    public class ManagerFactory
    {
        public static Manager GetCurrentUserAsManager(ICurrentUserContext currentUserContext)
        {
            return new Manager(currentUserContext.FullName, currentUserContext.Email,currentUserContext.Login, currentUserContext.Id);
        }
    }
}