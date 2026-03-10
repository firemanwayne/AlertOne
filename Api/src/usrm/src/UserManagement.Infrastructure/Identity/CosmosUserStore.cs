using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Custom user store implementation for Cosmos DB
/// Implements required Identity interfaces for user management
/// </summary>
internal sealed class CosmosUserStore :
    IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>,
    IUserLockoutStore<ApplicationUser>
{
    private readonly ApplicationDbContext _context;

    public CosmosUserStore(ApplicationDbContext context)
    {
        _context = context;
    }

    // IUserStore<ApplicationUser>

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        return await _context.Users.FindAsync([id], cancellationToken);
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.UserName = userName;

        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.LastModifiedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    // IUserPasswordStore<ApplicationUser>

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    // IUserEmailStore<ApplicationUser>

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.Email = email;

        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    // IUserRoleStore<ApplicationUser>

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpperInvariant(), cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found.");
        }

        var userRole = new IdentityUserRole<Guid>
        {
            UserId = user.Id,
            RoleId = role.Id
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name ?? string.Empty)
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpperInvariant(), cancellationToken);

        if (role is null)
        {
            return new List<ApplicationUser>();
        }

        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == role.Id)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpperInvariant(), cancellationToken);

        if (role is null)
        {
            return false;
        }

        return await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpperInvariant(), cancellationToken);

        if (role is null)
        {
            return;
        }

        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);

        if (userRole is not null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // IUserSecurityStampStore<ApplicationUser>

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    // IUserLockoutStore<ApplicationUser>

    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.LockoutEnabled);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.LockoutEnd);
    }

    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.AccessFailedCount++;

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.AccessFailedCount = 0;

        return Task.CompletedTask;
    }

    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.LockoutEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.LockoutEnd = lockoutEnd;

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // DbContext is managed by DI, no disposal needed here
    }
}
