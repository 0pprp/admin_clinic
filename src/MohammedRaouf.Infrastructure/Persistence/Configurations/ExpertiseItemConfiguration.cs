using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ExpertiseItemConfiguration : IEntityTypeConfiguration<ExpertiseItem>
{
    public void Configure(EntityTypeBuilder<ExpertiseItem> builder)
    {
        builder.ToTable("ExpertiseItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Title)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.IconKey)
            .HasMaxLength(64);

        builder.HasIndex(item => item.SortOrder);
    }
}
