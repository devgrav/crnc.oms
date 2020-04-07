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
                new Title("Some Sales Company", 
                    new NameAbbreviation("AS")), 
                new ContactPerson(
                    new FullName("John", "Galt"), 
                    new Email("some@mail.ru"), 
                    new Phone("+79153423345")));
            
            dbContext.Orders.Add(new Order(
                Guid.Parse("5c5c6017-1b1f-4a46-b423-455ad4f273fe"), 
                DateTime.Now, new JobInfo(JobType.New,"Develop new wall"), 
                customer,
                new Manager(new FullName("Shon", "Bean"), new Email("shon_bean@crnc.com"), 
                    "shon_bean",
                    Guid.Parse("b6ba35b2-adff-43a6-9cd7-b408240a6d6f"))
            ));

            dbContext.SaveChanges();
        }
    }
}