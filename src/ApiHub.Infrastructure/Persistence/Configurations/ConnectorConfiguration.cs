using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.BaseUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.ApiKeyHeaderName)
            .HasMaxLength(100);

        builder.Property(c => c.ApiKeyQueryParamName)
            .HasMaxLength(100);

        builder.Property(c => c.VersionHeaderName)
            .HasMaxLength(100);

        builder.Property(c => c.VersionQueryParamName)
            .HasMaxLength(100);

        builder.Property(c => c.DefaultVersion)
            .HasMaxLength(50);

        builder.HasMany(c => c.Endpoints)
            .WithOne(e => e.Connector)
            .HasForeignKey(e => e.ConnectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.UserSecrets)
            .WithOne(s => s.Connector)
            .HasForeignKey(s => s.ConnectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ApiRecords)
            .WithOne(r => r.Connector)
            .HasForeignKey(r => r.ConnectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Status);
    }
}
