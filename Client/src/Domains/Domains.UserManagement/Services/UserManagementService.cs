namespace Domains.UserManagement.Services;

/// <summary>
/// Service for user management operations
/// </summary>
internal sealed class UserManagementService(UserManagementApiClient apiClient) : IUserManagementService
{
    public async Task<IApiResult<IEnumerable<UserResponse>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await apiClient.GetAsync<IEnumerable<UserResponse>>(
            "/api/users",
            cancellationToken);
    }

    public async Task<IApiResult<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await apiClient.GetAsync<UserResponse>(
            $"/api/users/{userId}",
            cancellationToken);
    }

    public async Task<IApiResult<UserResponse>> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await apiClient.PutAsync<UpdateUserRequest, UserResponse>(
            $"/api/users/{userId}",
            request,
            cancellationToken);
    }

    public async Task<IApiResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await apiClient.DeleteAsync(
            $"/api/users/{userId}",
            cancellationToken);
    }
}
