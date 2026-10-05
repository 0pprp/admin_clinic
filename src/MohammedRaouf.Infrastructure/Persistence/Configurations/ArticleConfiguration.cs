using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(article => article.Id);

        builder.Property(article => article.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(article => article.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(article => article.Excerpt)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(article => article.Content)
            .IsRequired();

        builder.Property(article => article.CoverImage)
            .HasMaxLength(2048);

        builder.Property(article => article.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(article => article.Author)
            .WithMany()
            .HasForeignKey(article => article.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(article => article.Slug)
            .IsUnique();

        builder.HasIndex(article => article.Status);
    }
}
