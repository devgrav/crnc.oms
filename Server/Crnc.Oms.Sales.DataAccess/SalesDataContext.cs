using System;
using Crnc.Oms.Sales.DataAccess.Mappings;
using Crnc.Oms.Sales.Domain.Aggregates.Customers;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Sales.DataAccess
{
    /// <summary>
    /// EF Data context for sales
    /// </summary>
    public class SalesDataContext
    : DbContext
    {
        /// <summary>
        /// Customers aggregates set
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Orders aggregates set
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderMappingConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerMappingConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }


        public SalesDataContext(DbContextOptions<SalesDataContext> options)
            : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                Email = "some@mail.ru",
                Phone = "+79153423345",
                FullName = "John Galt"
            };
            
            Customers.Add(customer);

            SaveChanges();
            
            Orders.Add(new Order()
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

            SaveChanges();
        }
    }
}