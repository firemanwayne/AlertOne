using AlertManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlertManagement.Infrastructure.Data;

/// <summary>
/// Database context for alert management
/// </summary>
public sealed class AlertDbContext(DbContextOptions<AlertDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertType> AlertTypes => Set<AlertType>();
    public DbSet<AlertTypeUser> AlertTypeUsers => Set<AlertTypeUser>();
    public DbSet<AlertStatusLevel> AlertStatusLevels => Set<AlertStatusLevel>();
    public DbSet<AlertHistory> AlertHistories => Set<AlertHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alerts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
            entity.Property(e => e.RescindedBy).HasMaxLength(256);
            entity.HasOne(e => e.AlertType).WithMany().HasForeignKey(e => e.AlertTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AlertStatusLevel).WithMany().HasForeignKey(e => e.AlertStatusLevelId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AlertType>(entity =>
        {
            entity.ToTable("AlertTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1024).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<AlertTypeUser>(entity =>
        {
            entity.ToTable("AlertTypeUsers");
            entity.HasKey(e => new { e.AlertTypeId, e.UserId });
            entity.HasOne(e => e.AlertType)
            .WithMany(t => t.AlertTypeUsers)
            .HasForeignKey(e => e.AlertTypeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AlertStatusLevel>(entity =>
        {
            entity.ToTable("AlertStatusLevels");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1024).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<AlertHistory>(entity =>
        {
            entity.ToTable("AlertHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1024);
            entity.HasOne(e => e.Alert)
                .WithMany()
                .HasForeignKey(e => e.AlertId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.AlertId);
        });
    }
}
