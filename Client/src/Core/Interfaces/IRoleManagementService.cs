using AlertOne.Core.Common;
using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.DTOs.Responses;

namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for role management operations
/// </summary>
public interface IRoleManagementService
{
    /// <summary>
    /// Gets all roles
    /// </summary>
    Task<IApiResult<IEnumerable<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    Task<IApiResult> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    Task<IApiResult> RemoveRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role
    /// </summary>
    Task<IApiResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by ID
    /// </summary>
    Task<IApiResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
