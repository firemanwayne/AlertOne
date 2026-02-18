using UserManagementApps.Core.Common;
using UserManagementApps.Core.DTOs.Requests;
using UserManagementApps.Core.DTOs.Responses;
using UserManagementApps.Core.Interfaces;

namespace Domains.UserManagement.Services;

/// <summary>
/// Service for role management operations
/// </summary>
internal sealed class RoleManagementService(UserManagementApiClient apiClient) : IRoleManagementService
{
    public async Task<IApiResult<IEnumerable<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return await apiClient.GetAsync<IEnumerable<RoleResponse>>(
            "/api/roles",
            cancellationToken);
    }

    public async Task<IApiResult> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.PostAsync<AssignRoleRequest, object>(
            "/api/roles/assign",
            request,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Success();
        }

        return Result.Failure(result.Error!);
    }

    public async Task<IApiResult> RemoveRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.PostAsync<AssignRoleRequest, object>(
            "/api/roles/remove",
            request,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Success();
        }

        return Result.Failure(result.Error!);
    }
}
