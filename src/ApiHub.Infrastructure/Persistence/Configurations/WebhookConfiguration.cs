using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(w => w.Secret)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(w => w.Events)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(w => w.LastResponseStatus)
            .HasMaxLength(200);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.CreatedById);
        builder.HasIndex(w => w.IsEnabled);
        builder.HasIndex(w => w.Events);
    }
}
