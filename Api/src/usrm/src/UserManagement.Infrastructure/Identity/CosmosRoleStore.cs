using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Custom role store implementation for Cosmos DB
/// Implements required Identity interfaces for role management
/// </summary>
internal sealed class CosmosRoleStore : IRoleStore<IdentityRole<Guid>>
{
    /// <summary>
    /// The Cosmos DB context holding the roles.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes the store with the database context.
    /// </summary>
    public CosmosRoleStore(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> CreateAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role is null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> DeleteAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role is null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityRole<Guid>?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(roleId, out var id))
        {
            return null;
        }

        return await _context.Roles.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IdentityRole<Guid>?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.NormalizedName);
    }

    /// <inheritdoc />
    public Task<string> GetRoleIdAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.Id.ToString());
    }

    /// <inheritdoc />
    public Task<string?> GetRoleNameAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(role.Name);
    }

    /// <inheritdoc />
    public Task SetNormalizedRoleNameAsync(IdentityRole<Guid> role, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        role.NormalizedName = normalizedName;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetRoleNameAsync(IdentityRole<Guid> role, string? roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        role.Name = roleName;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> UpdateAsync(IdentityRole<Guid> role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role is null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // DbContext is managed by DI, no disposal needed here
    }
}
