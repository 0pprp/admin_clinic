using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.WhatsAppNumber)
            .HasMaxLength(32);

        builder.Property(user => user.Governorate)
            .HasMaxLength(100);

        builder.Property(user => user.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(user => user.AccountStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .IsRequired();

        builder.HasIndex(user => user.PhoneNumber)
            .HasDatabaseName("IX_AspNetUsers_PhoneNumber");
    }
}
