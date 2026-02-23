using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Database context for the application with Identity
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();

    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity table names
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        // Configure UserDevice
        builder.Entity<UserDevice>(entity =>
        {
            entity.ToTable("UserDevices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).HasMaxLength(256).IsRequired();
            entity.Property(e => e.DeviceName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Platform).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AppVersion).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.Property(e => e.RefreshToken).IsRequired();
            entity.Property(e => e.RefreshTokenExpiryTime).IsRequired();
            entity.Property(e => e.PushNotificationToken).HasMaxLength(512);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Location).HasMaxLength(256);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.RefreshToken).IsUnique();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserInvitation
        builder.Entity<UserInvitationEntity>(entity =>
        {
            entity.ToTable("UserInvitations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.ActivationCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.InvitedBy).HasMaxLength(256).IsRequired();
            entity.Property(e => e.InvitedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.ActivationCode).IsUnique();
        });

        // Seed initial roles
        SeedRoles(builder);
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        var adminRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var userRoleId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = adminRoleId.ToString()
            },
            new IdentityRole<Guid>
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = userRoleId.ToString()
            }
        );
    }
}
