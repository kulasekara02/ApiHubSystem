using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class RequestTemplateConfiguration : IEntityTypeConfiguration<RequestTemplate>
{
    public void Configure(EntityTypeBuilder<RequestTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Method)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.Endpoint)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Headers)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Body)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.QueryParams)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(t => t.Connector)
            .WithMany()
            .HasForeignKey(t => t.ConnectorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.CreatedById);
        builder.HasIndex(t => t.IsPublic);
    }
}
