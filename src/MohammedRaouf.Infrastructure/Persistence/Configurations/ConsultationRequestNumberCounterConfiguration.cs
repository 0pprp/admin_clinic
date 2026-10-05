using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ConsultationRequestNumberCounterConfiguration
    : IEntityTypeConfiguration<ConsultationRequestNumberCounter>
{
    public void Configure(EntityTypeBuilder<ConsultationRequestNumberCounter> builder)
    {
        builder.ToTable("ConsultationRequestNumberCounters");
        builder.HasKey(counter => counter.Year);
        builder.Property(counter => counter.Year).ValueGeneratedNever();
        builder.Property(counter => counter.LastValue).IsRequired();
    }
}
