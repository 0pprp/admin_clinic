using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests", table =>
        {
            table.HasCheckConstraint("CK_PurchaseRequests_AmountIQD", "\"AmountIQD\" >= 0");
        });

        builder.HasKey(request => request.Id);

        builder.Property(request => request.RequestNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(request => request.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.WhatsAppNumber)
            .HasMaxLength(32);

        builder.Property(request => request.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(request => request.Governorate)
            .HasMaxLength(100);

        builder.Property(request => request.PaymentMethod)
            .HasMaxLength(64);

        builder.Property(request => request.PaymentReference)
            .HasMaxLength(128);

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.CustomerNotes)
            .HasMaxLength(2000);

        builder.Property(request => request.AdminNotes)
            .HasMaxLength(4000);

        builder.HasOne(request => request.User)
            .WithMany()
            .HasForeignKey(request => request.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.Course)
            .WithMany(course => course.PurchaseRequests)
            .HasForeignKey(request => request.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(request => request.ConfirmedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.RequestNumber)
            .IsUnique();

        builder.HasIndex(request => new { request.UserId, request.CourseId })
            .IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Contacted', 'AwaitingPayment', 'PaymentReceived', 'ActivationCodeIssued')")
            .HasDatabaseName("UX_PurchaseRequests_OpenUserCourse");

        builder.HasIndex(request => request.UserId);
        builder.HasIndex(request => request.CourseId);
        builder.HasIndex(request => request.Status);
        builder.HasIndex(request => request.CreatedAt);
    }
}
