using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crnc.Oms.Sales.DataAccess.Mappings
{
    public class OrderMappingConfiguration
    : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(x => x.Number).HasMaxLength(100);


            builder.Ignore(x => x.DomainEvents);
            builder.OwnsOne(x => x.JobInfo, e =>
            {
                e.Property(x => x.Description).HasMaxLength(4000);
            });
            builder.OwnsOne(x => x.Status, e => { e.Property(x => x.DisplayName).HasMaxLength(200); });
            
            builder.OwnsOne(x => x.Customer, e =>
            {
                e.OwnsOne(x => x.Title,
                    e =>
                    {
                        e.OwnsOne(x => x.NameAbbreviation, 
                            e => e.Property(x => x.Value).HasMaxLength(300));
                        e.Property(x => x.Value).HasMaxLength(300);
                    });
                e.OwnsOne(x => x.ContactPerson,
                    e =>
                    {
                        e.OwnsOne(x => x.Email,
                            e => { e.Property(x => x.Value).HasMaxLength(300); });
                        e.OwnsOne(x => x.Phone,
                            e => { e.Property(x => x.Value).HasMaxLength(300); });
                        e.OwnsOne(x => x.FullName,
                            e =>
                            {
                                e.Property(x => x.FirstName).HasMaxLength(300);
                                e.Property(x => x.LastName).HasMaxLength(300);
                            });
                    });
            });
        }
    }
}