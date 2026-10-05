using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ActivationCodeConfiguration : IEntityTypeConfiguration<ActivationCode>
{
    public void Configure(EntityTypeBuilder<ActivationCode> builder)
    {
        builder.ToTable("ActivationCodes");

        builder.HasKey(code => code.Id);

        builder.Property(code => code.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(code => code.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(code => code.User)
            .WithMany()
            .HasForeignKey(code => code.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(code => code.Course)
            .WithMany(course => course.ActivationCodes)
            .HasForeignKey(code => code.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(code => code.PurchaseRequest)
            .WithMany(request => request.ActivationCodes)
            .HasForeignKey(code => code.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(code => code.CreatedByUser)
            .WithMany()
            .HasForeignKey(code => code.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(code => code.CodeHash)
            .IsUnique();

        builder.HasIndex(code => code.PurchaseRequestId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'")
            .HasDatabaseName("UX_ActivationCodes_ActivePurchaseRequest");

        builder.HasIndex(code => code.UserId);
        builder.HasIndex(code => code.Status);
    }
}
