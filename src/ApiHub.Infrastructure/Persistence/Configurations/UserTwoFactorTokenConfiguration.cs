using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiHub.Infrastructure.Persistence.Configurations;

public class UserTwoFactorTokenConfiguration : IEntityTypeConfiguration<UserTwoFactorToken>
{
    public void Configure(EntityTypeBuilder<UserTwoFactorToken> builder)
    {
        builder.ToTable("UserTwoFactorTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SecretKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.IsEnabled)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.RecoveryCodes)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.HasIndex(t => t.UserId)
            .IsUnique();

        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<UserTwoFactorToken>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
