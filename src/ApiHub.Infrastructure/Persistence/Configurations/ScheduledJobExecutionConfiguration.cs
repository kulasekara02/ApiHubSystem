using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class ScheduledJobExecutionConfiguration : IEntityTypeConfiguration<ScheduledJobExecution>
{
    public void Configure(EntityTypeBuilder<ScheduledJobExecution> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ResponseBody)
            .HasMaxLength(50000);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.StartedAt);
        builder.HasIndex(e => e.Status);
    }
}
