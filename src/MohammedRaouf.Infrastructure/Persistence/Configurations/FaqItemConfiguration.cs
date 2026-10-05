using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        builder.ToTable("FaqItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Question)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.Answer)
            .IsRequired();

        builder.HasIndex(item => item.SortOrder);
        builder.HasIndex(item => item.IsActive);
    }
}
