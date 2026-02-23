namespace Domains.UserManagement.Services;

/// <summary>
/// Service for user settings operations
/// </summary>
internal sealed class UserSettingsService(IUserManagementApiClient apiClient) : IUserSettingsService
{
    public async Task<IApiResult<UserSettingsResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .GetAsync<UserSettingsResponse>(
            $"/api/users/{userId}/settings",
            cancellationToken);
    }

    public async Task<IApiResult<UserSettingsResponse>> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .PutAsync<UpdateUserSettingsRequest, UserSettingsResponse>(
            $"/api/users/{userId}/settings",
            request,
            cancellationToken);
    }
}
