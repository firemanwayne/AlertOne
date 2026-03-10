using AlertManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlertManagement.Infrastructure.Data;

/// <summary>
/// Database context for alert management using Cosmos DB
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
            entity.ToContainer("alerts");
            entity.HasPartitionKey(e => e.AlertTypeId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.CreatedBy).IsRequired();
            // Navigation properties are not supported in Cosmos - we'll handle relationships manually
            entity.Ignore(e => e.AlertType);
            entity.Ignore(e => e.AlertStatusLevel);
        });

        builder.Entity<AlertType>(entity =>
        {
            entity.ToContainer("alert-types");
            entity.HasPartitionKey(e => e.Id);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.BackgroundColor).IsRequired();
            entity.Property(e => e.TextColor).IsRequired();
            // Navigation properties are not supported in Cosmos
            entity.Ignore(e => e.AlertTypeUsers);
        });

        builder.Entity<AlertTypeUser>(entity =>
        {
            entity.ToContainer("alert-type-users");
            entity.HasPartitionKey(e => e.AlertTypeId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            // Navigation properties are not supported in Cosmos
            entity.Ignore(e => e.AlertType);
        });

        builder.Entity<AlertStatusLevel>(entity =>
        {
            entity.ToContainer("alert-status-levels");
            entity.HasPartitionKey(e => e.Id);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.BackgroundColor).IsRequired();
            entity.Property(e => e.TextColor).IsRequired();
        });

        builder.Entity<AlertHistory>(entity =>
        {
            entity.ToContainer("alert-history");
            entity.HasPartitionKey(e => e.AlertId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.EventType).IsRequired();
            // Navigation properties are not supported in Cosmos
            entity.Ignore(e => e.Alert);
        });
    }
}
