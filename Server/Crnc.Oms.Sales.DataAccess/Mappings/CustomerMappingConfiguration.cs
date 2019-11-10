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
            builder.OwnsOne(x => x.Email, 
                e => { e.Property(x => x.Value).HasMaxLength(300); });
            builder.OwnsOne(x => x.Phone, 
                e => { e.Property(x => x.Value).HasMaxLength(300); });
            builder.OwnsOne(x => x.FullName,
                e =>
                {
                    e.Property(x => x.FirstName).HasMaxLength(300);
                    e.Property(x => x.MiddleName).HasMaxLength(300);
                    e.Property(x => x.LastName).HasMaxLength(300);
                });
        }
    }
}