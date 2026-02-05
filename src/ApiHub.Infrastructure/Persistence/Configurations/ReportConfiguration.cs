using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.TemplateType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.FilterCriteria)
            .HasColumnType("text");

        builder.Property(r => r.FilePath)
            .HasMaxLength(500);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => r.Schedule);
    }
}
