using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for role management operations
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role
    /// </summary>
    Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID
    /// </summary>
    Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by name
    /// </summary>
    Task<Result<RoleResponse>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles
    /// </summary>
    Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a role
    /// </summary>
    Task<Result<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role
    /// </summary>
    Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    Task<Result> AssignRoleToUserAsync(UserId userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    Task<Result> RemoveRoleFromUserAsync(UserId userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users in a specific role
    /// </summary>
    Task<Result<IEnumerable<UserResponse>>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);
}
