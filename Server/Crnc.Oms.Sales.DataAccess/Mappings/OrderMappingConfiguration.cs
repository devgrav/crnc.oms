using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crnc.Oms.Sales.DataAccess.Mappings
{
    public class OrderMappingConfiguration
    : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(x => x.Number).HasMaxLength(10);
            builder.Property(x => x.JobDescription).HasMaxLength(4000);
        }
    }
}