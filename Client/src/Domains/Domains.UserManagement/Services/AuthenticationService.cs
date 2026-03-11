using Microsoft.Extensions.Logging;

namespace Domains.UserManagement.Services;

/// <summary>
/// Service for user authentication operations
/// </summary>
internal sealed class AuthenticationService(
    IUserManagementApiClient apiClient,
    ITokenStorageService tokenStorage,
    AppState appState,
    IPushNotificationRegistrationService? pushNotificationService = null,
    ILogger<AuthenticationService>? logger = null) : IAuthenticationService
{
    public async Task<IApiResult<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.PostAsync<LoginRequest, AuthenticationResponse>(
            "/api/auth/login",
            request,
            cancellationToken);

        if (result.IsSuccess)
        {
            var authResponse = result.Value;

            // Store tokens
            await tokenStorage.SaveTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken,
                authResponse.ExpiresAt);

            // Update AppState
            appState.AccessToken = authResponse.AccessToken;
            appState.CurrentUser = new UserProfile
            {
                Id = authResponse.User.Id,
                Email = authResponse.User.Email,
                FirstName = authResponse.User.FirstName,
                LastName = authResponse.User.LastName,
                Roles = [.. authResponse.User.Roles]
            };
            appState.IsAuthenticated = true;

            // Register for push notifications (fire and forget, don't block login)
            _ = RegisterForPushNotificationsAsync(authResponse.User.Id.ToString(), authResponse.User.Roles, cancellationToken);
        }

        return result;
    }

    private async Task RegisterForPushNotificationsAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        if (pushNotificationService is null || !pushNotificationService.IsSupported)
        {
            return;
        }

        try
        {
            // Build tags from user roles
            var tags = roles.Select(r => $"role:{r.ToLowerInvariant()}").ToList();

            var registrationId = await pushNotificationService.RegisterAsync(userId, tags, cancellationToken);

            if (registrationId is not null)
            {
                logger?.LogInformation("Push notification registration successful. RegistrationId: {RegistrationId}", registrationId);
            }
            else
            {
                logger?.LogWarning("Push notification registration failed");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error registering for push notifications");
        }
    }

    public async Task<IApiResult<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.PostAsync<RegisterRequest, AuthenticationResponse>(
            "api/auth/register",
            request,
            cancellationToken);

        if (result.IsSuccess)
        {
            var authResponse = result.Value;

            // Store tokens
            await tokenStorage
                .SaveTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken,
                authResponse.ExpiresAt);

            // Update AppState
            appState.AccessToken = authResponse.AccessToken;
            appState.CurrentUser = new UserProfile
            {
                Id = authResponse.User.Id,
                Email = authResponse.User.Email,
                FirstName = authResponse.User.FirstName,
                LastName = authResponse.User.LastName,
                Roles = [.. authResponse.User.Roles]
            };
            appState.IsAuthenticated = true;

            // Register for push notifications (fire and forget, don't block registration)
            _ = RegisterForPushNotificationsAsync(authResponse.User.Id.ToString(), authResponse.User.Roles, cancellationToken);
        }

        return result;
    }

    public async Task<IApiResult<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient
            .PostAsync<RefreshTokenRequest, AuthenticationResponse>(
            "/api/auth/refresh",
            request,
            cancellationToken);

        if (result.IsSuccess)
        {
            var authResponse = result.Value;

            // Store new tokens
            await tokenStorage.SaveTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken,
                authResponse.ExpiresAt);

            // Update AppState
            appState.AccessToken = authResponse.AccessToken;
        }

        return result;
    }

    public async Task<IApiResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        // Unregister from push notifications
        await UnregisterFromPushNotificationsAsync(cancellationToken);

        // Clear tokens from storage
        await tokenStorage.ClearTokensAsync();

        // Clear AppState
        appState.Clear();

        return Result.Success();
    }

    public async Task<IApiResult<bool>> HasUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = await apiClient.GetAsync<HasUsersResponse>(
            "/api/auth/has-users",
            cancellationToken);

        if (result.IsSuccess)
        {
            return Result<bool>.Success(result.Value.HasUsers);
        }

        return Result<bool>.Failure(result.Error ?? Error.Internal("HasUsers.Failed", "Failed to check for users"));
    }

    private record HasUsersResponse(bool HasUsers);

    private async Task UnregisterFromPushNotificationsAsync(CancellationToken cancellationToken)
    {
        if (pushNotificationService is null || !pushNotificationService.IsRegistered)
        {
            return;
        }

        try
        {
            var success = await pushNotificationService.UnregisterAsync(cancellationToken);

            if (success)
            {
                logger?.LogInformation("Push notification unregistration successful");
            }
            else
            {
                logger?.LogWarning("Push notification unregistration failed");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error unregistering from push notifications");
        }
    }
}
