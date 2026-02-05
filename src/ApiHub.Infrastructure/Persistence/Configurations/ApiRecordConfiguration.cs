using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class ApiRecordConfiguration : IEntityTypeConfiguration<ApiRecord>
{
    public void Configure(EntityTypeBuilder<ApiRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.CorrelationId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.RequestUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.RequestHeaders)
            .HasColumnType("text");

        builder.Property(r => r.RequestBody)
            .HasColumnType("text");

        builder.Property(r => r.ResponseHeaders)
            .HasColumnType("text");

        builder.Property(r => r.ResponseBody)
            .HasColumnType("text");

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(r => r.Dataset)
            .WithOne(d => d.ApiRecord)
            .HasForeignKey<Dataset>(d => d.ApiRecordId);

        builder.HasIndex(r => r.CorrelationId);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ConnectorId);
        builder.HasIndex(r => r.IsSuccess);
        builder.HasIndex(r => r.StatusCode);
    }
}
