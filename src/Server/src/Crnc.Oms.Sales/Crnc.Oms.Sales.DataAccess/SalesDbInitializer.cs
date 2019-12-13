using System;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.DataAccess
{
    public static class SalesDbInitializer
    {
        public static void Initialize(SalesDataContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var customer = new Customer( 
                new FullName("John", "Galt"),
                new NameAbbreviation("JG"), 
                new Email("some@mail.ru"),
                new Phone("+79153423345"));
            
            dbContext.Orders.Add(new Order(
                Guid.NewGuid(), 
                DateTime.Now, 
                JobType.New, 
                "Develop new wall", 
                customer)
            );

            dbContext.SaveChanges();
        }
    }
}