using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("CourseEnrollments");

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(enrollment => enrollment.User)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.Course)
            .WithMany(course => course.Enrollments)
            .HasForeignKey(enrollment => enrollment.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.PurchaseRequest)
            .WithOne(request => request.Enrollment)
            .HasForeignKey<CourseEnrollment>(enrollment => enrollment.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.ActivationCode)
            .WithOne(code => code.Enrollment)
            .HasForeignKey<CourseEnrollment>(enrollment => enrollment.ActivationCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.ActivatedByUser)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.ActivatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(enrollment => new { enrollment.UserId, enrollment.CourseId })
            .IsUnique();

        builder.HasIndex(enrollment => enrollment.CourseId);
        builder.HasIndex(enrollment => enrollment.Status);
    }
}
