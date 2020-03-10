using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crnc.Oms.Sales.DataAccess.Mappings
{
    public class ManagerMappingConfiguration
        : IEntityTypeConfiguration<Manager>
    {
        public void Configure(EntityTypeBuilder<Manager> builder)
        {
            builder.OwnsOne(x => x.FullName);
            builder.OwnsOne(x => x.Email);
        }
    }
}