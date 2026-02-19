namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for storing and retrieving authentication tokens
/// </summary>
public interface ITokenStorageService
{
    /// <summary>
    /// Gets the stored access token
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Gets the stored refresh token
    /// </summary>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>
    /// Gets the token expiry time
    /// </summary>
    Task<DateTime?> GetExpiryTimeAsync();

    /// <summary>
    /// Saves the authentication tokens
    /// </summary>
    Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);

    /// <summary>
    /// Clears all stored tokens
    /// </summary>
    Task ClearTokensAsync();

    /// <summary>
    /// Checks if the stored token is expired
    /// </summary>
    Task<bool> IsTokenExpiredAsync();
}
