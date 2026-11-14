using AlertOne.Core.Interfaces;
using Blazored.LocalStorage;

namespace AlertOne.Web.Services;

/// <summary>
/// Browser-specific token storage implementation using LocalStorage
/// </summary>
internal sealed class BrowserTokenStorageService(ILocalStorageService localStorage) : ITokenStorageService
{
    /// <summary>
    /// Local storage key of the access token.
    /// </summary>
    private const string AccessTokenKey = "access_token";
    /// <summary>
    /// Local storage key of the refresh token.
    /// </summary>
    private const string RefreshTokenKey = "refresh_token";
    /// <summary>
    /// Local storage key of the token expiry time.
    /// </summary>
    private const string ExpiryTimeKey = "expiry_time";

    /// <inheritdoc />
    public async Task<string?> GetAccessTokenAsync()
    {
        return await localStorage.GetItemAsync<string>(AccessTokenKey);
    }

    /// <inheritdoc />
    public async Task<string?> GetRefreshTokenAsync()
    {
        return await localStorage.GetItemAsync<string>(RefreshTokenKey);
    }

    /// <inheritdoc />
    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        await localStorage.SetItemAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, refreshToken);
        await localStorage.SetItemAsync(ExpiryTimeKey, expiresAt.ToString("O"));
    }

    /// <inheritdoc />
    public async Task ClearTokensAsync()
    {
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(ExpiryTimeKey);
    }

    /// <inheritdoc />
    public async Task<bool> IsTokenExpiredAsync()
    {
        var expiryTime = await GetExpiryTimeAsync();
        return !expiryTime.HasValue || expiryTime.Value <= DateTime.UtcNow;
    }

    /// <inheritdoc />
    public async Task<DateTime?> GetExpiryTimeAsync()
    {
        var expiryString = await localStorage.GetItemAsync<string>(ExpiryTimeKey);
        if (string.IsNullOrEmpty(expiryString))
        {
            return null;
        }

        if (DateTime.TryParse(expiryString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiryTime))
        {
            // Ensure we return UTC for proper comparison
            return expiryTime.Kind == DateTimeKind.Utc
                ? expiryTime
                : expiryTime.ToUniversalTime();
        }

        return null;
    }
}
