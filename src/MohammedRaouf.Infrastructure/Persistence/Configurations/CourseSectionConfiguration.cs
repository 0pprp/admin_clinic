using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("CourseSections");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(section => section.Description)
            .HasMaxLength(2000);

        builder.HasOne(section => section.Course)
            .WithMany(course => course.Sections)
            .HasForeignKey(section => section.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(section => new { section.CourseId, section.SortOrder });
    }
}
