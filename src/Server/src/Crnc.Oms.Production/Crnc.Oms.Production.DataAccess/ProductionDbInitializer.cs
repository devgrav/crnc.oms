using System;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Aggregates.Order;

namespace Crnc.Oms.Production.DataAccess
{
    public static class ProductionDbInitializer
    {
        public static void Initialize(ProductionDataContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            dbContext.Jobs.Add(new Job(
                Guid.Parse("f425e777-1d53-40d3-99dd-d51e1a72fafa"), 
                DateTime.Now, 
                JobType.New, 
                "Develop new wall",
                new Manager("Shon Bean", "shon_bean"), 
                Guid.Parse("5c5c6017-1b1f-4a46-b423-455ad4f273fe"),
                "5c5c6017")); 
            
            dbContext.SaveChanges();
        }
    }
}