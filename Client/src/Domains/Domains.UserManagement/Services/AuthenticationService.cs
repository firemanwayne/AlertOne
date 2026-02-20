namespace Domains.UserManagement.Services;

/// <summary>
/// Service for user authentication operations
/// </summary>
internal sealed class AuthenticationService(
    UserManagementApiClient apiClient,
    ITokenStorageService tokenStorage,
    AppState appState) : IAuthenticationService
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
        }

        return result;
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
        // Clear tokens from storage
        await tokenStorage.ClearTokensAsync();

        // Clear AppState
        appState.Clear();

        return Result.Success();
    }
}
