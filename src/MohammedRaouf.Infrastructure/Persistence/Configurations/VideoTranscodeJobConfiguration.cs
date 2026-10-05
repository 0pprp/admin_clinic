using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class VideoTranscodeJobConfiguration : IEntityTypeConfiguration<VideoTranscodeJob>
{
    public void Configure(EntityTypeBuilder<VideoTranscodeJob> builder)
    {
        builder.ToTable("VideoTranscodeJobs");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.SourceRelativePath).HasMaxLength(500).IsRequired();
        builder.Property(item => item.OutputRelativePath).HasMaxLength(500);
        builder.Property(item => item.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(item => new { item.Status, item.CreatedAt });
        builder.HasIndex(item => item.LessonId);
        builder.HasOne(item => item.Lesson)
            .WithMany()
            .HasForeignKey(item => item.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
