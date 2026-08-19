using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crnc.Oms.Production.DataAccess.Mappings
{
    public class JobMappingConfiguration
    : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.Property(x => x.Number).HasMaxLength(100);
            builder.Property(x => x.JobDescription).HasMaxLength(4000);

            builder.OwnsOne(x => x.Manager, b =>
            {
                b.Property(x => x.Login).HasMaxLength(200);
                b.Property(x => x.FullName).HasMaxLength(200);
            });

            builder.Ignore(x => x.DomainEvents);
        }
    }
}