using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.DataAccess.Mappings;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.SeedWork;
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
        /// Orders aggregates set
        /// </summary>
        public DbSet<Order> Orders { get; set; }
        
        /// <summary>
        /// Managers set
        /// </summary>
        public DbSet<Manager> Managers { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderMappingConfiguration());
            modelBuilder.ApplyConfiguration(new ManagerMappingConfiguration());
        }

        public SalesDataContext(DbContextOptions<SalesDataContext> options)
            : base(options)
        {

        }
    }
}