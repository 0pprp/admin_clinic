using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class PurchaseRequestEventConfiguration : IEntityTypeConfiguration<PurchaseRequestEvent>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestEvent> builder)
    {
        builder.ToTable("PurchaseRequestEvents");

        builder.HasKey(requestEvent => requestEvent.Id);

        builder.Property(requestEvent => requestEvent.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(requestEvent => requestEvent.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(requestEvent => requestEvent.Note)
            .HasMaxLength(2000);

        builder.HasOne(requestEvent => requestEvent.PurchaseRequest)
            .WithMany(request => request.Events)
            .HasForeignKey(requestEvent => requestEvent.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(requestEvent => requestEvent.ActorUser)
            .WithMany()
            .HasForeignKey(requestEvent => requestEvent.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(requestEvent => requestEvent.PurchaseRequestId);
        builder.HasIndex(requestEvent => requestEvent.CreatedAt);
    }
}
