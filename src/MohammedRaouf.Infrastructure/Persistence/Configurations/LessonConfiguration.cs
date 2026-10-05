using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons", table =>
        {
            table.HasCheckConstraint("CK_Lessons_DurationSeconds", "\"DurationSeconds\" >= 0");
        });

        builder.HasKey(lesson => lesson.Id);

        builder.Property(lesson => lesson.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lesson => lesson.Description)
            .HasMaxLength(4000);

        builder.Property(lesson => lesson.VideoProvider)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(lesson => lesson.VideoKey)
            .HasMaxLength(500);

        builder.Property(lesson => lesson.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(lesson => lesson.CourseSection)
            .WithMany(section => section.Lessons)
            .HasForeignKey(lesson => lesson.CourseSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lesson => new { lesson.CourseSectionId, lesson.SortOrder });
    }
}
