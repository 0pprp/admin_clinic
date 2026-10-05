using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class SiteStatisticConfiguration : IEntityTypeConfiguration<SiteStatistic>
{
    public void Configure(EntityTypeBuilder<SiteStatistic> builder)
    {
        builder.ToTable("SiteStatistics");

        builder.HasKey(statistic => statistic.Id);

        builder.Property(statistic => statistic.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(statistic => statistic.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(statistic => statistic.DisplayValue)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(statistic => statistic.Key)
            .IsUnique();
    }
}
