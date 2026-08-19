using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Production.DataAccess.Mappings;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Production.DataAccess
{
    /// <summary>
    /// EF Data context for sales
    /// </summary>
    public class ProductionDataContext
    : DbContext
    {
        /// <summary>
        /// Job aggregates set
        /// </summary>
        public DbSet<Job> Jobs { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new JobMappingConfiguration());

        }

        public ProductionDataContext(DbContextOptions<ProductionDataContext> options)
            : base(options)
        {

        }
    }
}