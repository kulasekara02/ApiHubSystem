using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Payload)
            .IsRequired();

        builder.Property(d => d.RequestHeaders)
            .HasMaxLength(2000);

        builder.Property(d => d.ResponseBody)
            .HasMaxLength(10000);

        builder.Property(d => d.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(d => d.Webhook)
            .WithMany()
            .HasForeignKey(d => d.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.WebhookId);
        builder.HasIndex(d => d.TriggeredAt);
        builder.HasIndex(d => d.IsSuccess);
        builder.HasIndex(d => new { d.WebhookId, d.TriggeredAt });
    }
}
