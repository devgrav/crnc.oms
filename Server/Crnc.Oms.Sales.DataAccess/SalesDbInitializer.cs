using System;
using Crnc.Oms.Sales.Domain.Aggregates.Customers;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;

namespace Crnc.Oms.Sales.DataAccess
{
    public static class SalesDbInitializer
    {
        public static void Initialize(SalesDataContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                Email = "some@mail.ru",
                Phone = "+79153423345",
                FullName = "John Galt"
            };
            
            dbContext.Customers.Add(customer);

            dbContext.SaveChanges();
            
            dbContext.Orders.Add(new Order()
            {
                Customer = customer,
                Id = Guid.NewGuid(),
                Number = "O-00001",
                Status = OrderStatus.NeedSignoff,
                DateCreated = DateTime.Now,
                JobDescription = "Develop new wall",
                JobType = JobType.New,
                MaterialSource = MaterialSource.UserStock,
                SignOffType = SignoffType.Email,
                DateSentToCustomer = null
            });

            dbContext.SaveChanges();
        }
    }
}