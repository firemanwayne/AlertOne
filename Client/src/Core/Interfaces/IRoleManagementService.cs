using UserManagementApps.Core.Common;
using UserManagementApps.Core.DTOs.Requests;
using UserManagementApps.Core.DTOs.Responses;

namespace UserManagementApps.Core.Interfaces;

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
}
