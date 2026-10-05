using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("LessonProgress", table =>
        {
            table.HasCheckConstraint("CK_LessonProgress_WatchedSeconds", "\"WatchedSeconds\" >= 0");
        });

        builder.HasKey(progress => progress.Id);

        builder.HasOne(progress => progress.User)
            .WithMany()
            .HasForeignKey(progress => progress.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(progress => progress.Lesson)
            .WithMany(lesson => lesson.ProgressRecords)
            .HasForeignKey(progress => progress.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(progress => new { progress.UserId, progress.LessonId })
            .IsUnique();

        builder.HasIndex(progress => progress.LessonId);
    }
}
