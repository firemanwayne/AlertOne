using UserManagementApps.Core.Common;
using UserManagementApps.Core.DTOs.Requests;
using UserManagementApps.Core.DTOs.Responses;

namespace UserManagementApps.Core.Interfaces;

/// <summary>
/// Service for user authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    Task<IApiResult<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user account
    /// </summary>
    Task<IApiResult<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the access token using a refresh token
    /// </summary>
    Task<IApiResult<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task<IApiResult> LogoutAsync(CancellationToken cancellationToken = default);
}
