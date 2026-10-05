using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MohammedRaouf.Domain.Entities;

namespace MohammedRaouf.Infrastructure.Persistence.Configurations;

public sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable("AdminAuditLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(log => log.MetadataJson)
            .HasColumnType("jsonb");

        builder.Property(log => log.IpAddress)
            .HasMaxLength(64);

        builder.HasOne(log => log.AdminUser)
            .WithMany()
            .HasForeignKey(log => log.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.EntityType, log.EntityId });
    }
}
