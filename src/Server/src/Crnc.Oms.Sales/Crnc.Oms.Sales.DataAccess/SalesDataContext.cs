using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.DataAccess.Mappings;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crnc.Oms.Sales.DataAccess
{
    /// <summary>
    /// EF Data context for sales
    /// </summary>
    public class SalesDataContext
    : DbContext
    {
        /// <summary>
        /// Orders aggregates set
        /// </summary>
        public DbSet<Order> Orders { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderMappingConfiguration());
            modelBuilder.ApplyConfiguration(new ManagerMappingConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSnakeCaseNamingConvention();
        }


        public SalesDataContext(DbContextOptions<SalesDataContext> options)
            : base(options)
        {

        }
    }
}