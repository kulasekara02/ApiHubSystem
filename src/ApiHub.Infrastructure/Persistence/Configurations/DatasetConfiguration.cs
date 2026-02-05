using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
{
    public void Configure(EntityTypeBuilder<Dataset> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.JsonSnapshot)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(d => d.Schema)
            .HasColumnType("text");

        builder.HasIndex(d => d.ApiRecordId)
            .IsUnique();
    }
}
