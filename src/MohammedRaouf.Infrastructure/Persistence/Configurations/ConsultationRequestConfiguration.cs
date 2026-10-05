using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ConsultationRequestConfiguration : IEntityTypeConfiguration<ConsultationRequest>
{
    public void Configure(EntityTypeBuilder<ConsultationRequest> builder)
    {
        builder.ToTable("ConsultationRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.RequestNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(request => request.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.WhatsAppNumber)
            .HasMaxLength(32);

        builder.Property(request => request.Email)
            .HasMaxLength(256);

        builder.Property(request => request.ConsultationType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.CompanyName)
            .HasMaxLength(200);

        builder.Property(request => request.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(request => request.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(request => request.PreferredCommunicationMethod)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.AdminNotes)
            .HasMaxLength(4000);

        builder.HasOne(request => request.User)
            .WithMany()
            .HasForeignKey(request => request.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.RequestNumber)
            .IsUnique();

        builder.HasIndex(request => request.Status);
        builder.HasIndex(request => request.CreatedAt);
    }
}
