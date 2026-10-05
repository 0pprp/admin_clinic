using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", table =>
        {
            table.HasCheckConstraint("CK_Courses_PriceIQD", "\"PriceIQD\" >= 0");
            table.HasCheckConstraint(
                "CK_Courses_AccessDuration",
                "(\"AccessType\" = 'Lifetime' AND \"AccessDurationDays\" IS NULL) OR (\"AccessType\" = 'LimitedDuration' AND \"AccessDurationDays\" > 0)");
        });

        builder.HasKey(course => course.Id);

        builder.Property(course => course.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(course => course.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(course => course.ShortDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(course => course.Description)
            .IsRequired();

        builder.Property(course => course.PriceIQD)
            .IsRequired();

        builder.Property(course => course.ThumbnailUrl)
            .HasMaxLength(2048);

        builder.Property(course => course.TrailerUrl)
            .HasMaxLength(2048);

        builder.Property(course => course.Level)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(course => course.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(course => course.AccessType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(course => course.Slug)
            .IsUnique();

        builder.HasIndex(course => course.Status);
    }
}
