using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.Description)
            .HasMaxLength(500);

        builder.Property(j => j.Endpoint)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(j => j.Method)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(j => j.Headers)
            .HasMaxLength(4000);

        builder.Property(j => j.Body)
            .HasMaxLength(10000);

        builder.Property(j => j.CronExpression)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.LastRunStatus)
            .HasMaxLength(50);

        builder.HasOne(j => j.Connector)
            .WithMany()
            .HasForeignKey(j => j.ConnectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.CreatedBy)
            .WithMany()
            .HasForeignKey(j => j.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.Executions)
            .WithOne(e => e.ScheduledJob)
            .HasForeignKey(e => e.ScheduledJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => j.Name);
        builder.HasIndex(j => j.IsEnabled);
        builder.HasIndex(j => j.NextRunAt);
    }
}
