using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class EmailOtpCodeConfiguration : IEntityTypeConfiguration<EmailOtpCode>
{
    public void Configure(EntityTypeBuilder<EmailOtpCode> builder)
    {
        builder.ToTable("EmailOtpCodes");

        builder.HasKey(code => code.Id);

        builder.Property(code => code.Purpose)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(code => code.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasOne(code => code.User)
            .WithMany()
            .HasForeignKey(code => code.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(code => new { code.UserId, code.Purpose, code.ConsumedAt });
        builder.HasIndex(code => code.ExpiresAt);
    }
}
