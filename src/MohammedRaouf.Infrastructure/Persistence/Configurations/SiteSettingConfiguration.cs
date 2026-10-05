using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.ToTable("SiteSettings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(setting => setting.ValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(setting => setting.UpdatedByUser)
            .WithMany()
            .HasForeignKey(setting => setting.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(setting => setting.Key)
            .IsUnique();
    }
}
