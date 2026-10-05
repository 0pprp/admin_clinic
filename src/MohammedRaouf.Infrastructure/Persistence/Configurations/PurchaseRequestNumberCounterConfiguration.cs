using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class PurchaseRequestNumberCounterConfiguration : IEntityTypeConfiguration<PurchaseRequestNumberCounter>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestNumberCounter> builder)
    {
        builder.ToTable("PurchaseRequestNumberCounters");
        builder.HasKey(counter => counter.Year);
        builder.Property(counter => counter.Year).ValueGeneratedNever();
        builder.Property(counter => counter.LastValue).IsRequired();
    }
}
