using AuditLogSample.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Data;

public class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasMaxLength(64);
            entity.Property(user => user.Name).IsRequired().HasMaxLength(100);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(256);
            entity.Property(user => user.MobileNo).HasMaxLength(32);
            entity.Property(user => user.Status).IsRequired().HasMaxLength(32);
            entity.Property(user => user.Limit).HasColumnType("decimal(18,2)");
            entity.HasIndex(user => user.Email);
            entity.HasIndex(user => user.MobileNo);
            entity.HasIndex(user => user.CreatedAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Id).HasMaxLength(64);
            entity.Property(log => log.ActorUserId).IsRequired().HasMaxLength(64);
            entity.Property(log => log.ActorRole).HasMaxLength(64);
            entity.Property(log => log.Action).IsRequired().HasMaxLength(32);
            entity.Property(log => log.ResourceType).IsRequired().HasMaxLength(64);
            entity.Property(log => log.TargetId).IsRequired().HasMaxLength(64);
            entity.Property(log => log.TargetLookupKey).HasMaxLength(256);
            entity.Property(log => log.TargetUserId).HasMaxLength(64);
            entity.Property(log => log.TargetUserName).HasMaxLength(100);
            entity.Property(log => log.TargetUserMobileNo).HasMaxLength(32);
            entity.HasIndex(log => log.ActionDateTime);
            entity.HasIndex(log => log.TargetId);
            entity.HasIndex(log => log.TargetUserId);
        });
    }
}
