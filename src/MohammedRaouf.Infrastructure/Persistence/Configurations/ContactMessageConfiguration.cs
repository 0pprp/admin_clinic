using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Phone)
            .HasMaxLength(32);

        builder.Property(message => message.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(message => message.Subject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(message => message.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(message => message.User)
            .WithMany()
            .HasForeignKey(message => message.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(message => message.Status);
        builder.HasIndex(message => message.CreatedAt);
    }
}
