using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and returns tokens
    /// </summary>
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a user's refresh token
    /// </summary>
    Task<Result> RevokeTokenAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any users exist in the system
    /// </summary>
    Task<Result<bool>> HasUsersAsync(CancellationToken cancellationToken = default);
}
