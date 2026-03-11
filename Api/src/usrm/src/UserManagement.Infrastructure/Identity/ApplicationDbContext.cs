using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Database context for the application with Identity using Cosmos DB
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<IdentityRole<Guid>> Roles => Set<IdentityRole<Guid>>();
    public DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
    public DbSet<IdentityUserClaim<Guid>> UserClaims => Set<IdentityUserClaim<Guid>>();
    public DbSet<IdentityUserLogin<Guid>> UserLogins => Set<IdentityUserLogin<Guid>>();
    public DbSet<IdentityRoleClaim<Guid>> RoleClaims => Set<IdentityRoleClaim<Guid>>();
    public DbSet<IdentityUserToken<Guid>> UserTokens => Set<IdentityUserToken<Guid>>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToContainer("users");
            entity.HasPartitionKey(e => e.Id);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure IdentityRole
        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToContainer("roles");
            entity.HasPartitionKey(e => e.Id);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
        });

        // Configure IdentityUserRole
        // Cosmos DB requires a single string 'id' property - use shadow property combining UserId and RoleId
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToContainer("user-roles");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();

            // Create a shadow property for the Cosmos id
            entity.Property<string>("Id")
                .ToJsonProperty("id")
                .HasValueGenerator<UserRoleIdGenerator>();
            entity.HasKey("Id");

            entity.Property(e => e.UserId).ToJsonProperty("userId");
            entity.Property(e => e.RoleId).ToJsonProperty("roleId");
        });

        // Configure IdentityUserClaim
        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToContainer("user-claims");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ToJsonProperty("id")
                .HasConversion<string>();
            entity.Property(e => e.UserId).ToJsonProperty("userId");
        });

        // Configure IdentityUserLogin
        // Cosmos DB requires a single string 'id' property - use shadow property combining LoginProvider and ProviderKey
        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToContainer("user-logins");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();

            // Create a shadow property for the Cosmos id
            entity.Property<string>("Id")
                .ToJsonProperty("id")
                .HasValueGenerator<UserLoginIdGenerator>();
            entity.HasKey("Id");

            entity.Property(e => e.UserId).ToJsonProperty("userId");
            entity.Property(e => e.LoginProvider).ToJsonProperty("loginProvider");
            entity.Property(e => e.ProviderKey).ToJsonProperty("providerKey");
        });

        // Configure IdentityRoleClaim
        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToContainer("role-claims");
            entity.HasPartitionKey(e => e.RoleId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ToJsonProperty("id")
                .HasConversion<string>();
            entity.Property(e => e.RoleId).ToJsonProperty("roleId");
        });

        // Configure IdentityUserToken
        // Cosmos DB requires a single string 'id' property - use shadow property combining UserId, LoginProvider, and Name
        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToContainer("user-tokens");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();

            // Create a shadow property for the Cosmos id
            entity.Property<string>("Id")
                .ToJsonProperty("id")
                .HasValueGenerator<UserTokenIdGenerator>();
            entity.HasKey("Id");

            entity.Property(e => e.UserId).ToJsonProperty("userId");
            entity.Property(e => e.LoginProvider).ToJsonProperty("loginProvider");
            entity.Property(e => e.Name).ToJsonProperty("name");
        });

        // Configure UserDevice
        builder.Entity<UserDevice>(entity =>
        {
            entity.ToContainer("user-devices");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.UserId).ToJsonProperty("userId");
            entity.Property(e => e.DeviceId).IsRequired();
            entity.Property(e => e.DeviceName).IsRequired();
            entity.Property(e => e.Platform).IsRequired();
            entity.Property(e => e.RefreshToken).IsRequired();
            entity.Property(e => e.RefreshTokenExpiryTime).IsRequired();
            // Navigation properties are not supported in Cosmos
            entity.Ignore(e => e.User);
        });

        // Configure UserInvitation
        builder.Entity<UserInvitationEntity>(entity =>
        {
            entity.ToContainer("user-invitations");
            entity.HasPartitionKey(e => e.Email);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.Email).ToJsonProperty("email");
            entity.Property(e => e.ActivationCode).IsRequired();
            entity.Property(e => e.InvitedBy).IsRequired();
            entity.Property(e => e.InvitedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
        });

        // Configure UserSettings
        builder.Entity<UserSettings>(entity =>
        {
            entity.ToContainer("user-settings");
            entity.HasPartitionKey(e => e.UserId);
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ToJsonProperty("id");
            entity.Property(e => e.UserId).ToJsonProperty("userId");
            entity.Property(e => e.ReceiveEmailNotifications).IsRequired();
            entity.Property(e => e.ReceivePushNotifications).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedDate).IsRequired();
            // Navigation properties are not supported in Cosmos
            entity.Ignore(e => e.User);
        });
    }
}
