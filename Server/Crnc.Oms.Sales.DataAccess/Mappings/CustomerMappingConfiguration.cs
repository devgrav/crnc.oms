using Crnc.Oms.Sales.Domain.Aggregates.Customers;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crnc.Oms.Sales.DataAccess.Mappings
{
    public class CustomerMappingConfiguration
    : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(x => x.FullName).HasMaxLength(300);
            builder.Property(x => x.Email).HasMaxLength(100);
            builder.Property(x => x.Phone).HasMaxLength(100);
        }
    }
}