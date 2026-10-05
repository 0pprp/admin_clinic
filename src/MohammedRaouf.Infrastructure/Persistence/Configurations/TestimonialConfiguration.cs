using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");

        builder.HasKey(testimonial => testimonial.Id);

        builder.Property(testimonial => testimonial.AuthorDisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(testimonial => testimonial.AuthorTitle)
            .HasMaxLength(200);

        builder.Property(testimonial => testimonial.Body)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(testimonial => new { testimonial.IsPublished, testimonial.SortOrder });
    }
}
