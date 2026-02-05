using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.OriginalFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.ParsedDataJson)
            .HasColumnType("text");

        builder.Property(f => f.ColumnHeaders)
            .HasMaxLength(4000);

        builder.Property(f => f.ProcessingError)
            .HasMaxLength(2000);

        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.CreatedAt);
        builder.HasIndex(f => f.FileType);
    }
}
