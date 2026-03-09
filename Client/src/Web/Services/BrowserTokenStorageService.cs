using AlertOne.Core.Interfaces;
using Blazored.LocalStorage;

namespace AlertOne.Web.Services;

/// <summary>
/// Browser-specific token storage implementation using LocalStorage
/// </summary>
internal sealed class BrowserTokenStorageService(ILocalStorageService localStorage) : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiryTimeKey = "expiry_time";

    public async Task<string?> GetAccessTokenAsync()
    {
        return await localStorage.GetItemAsync<string>(AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await localStorage.GetItemAsync<string>(RefreshTokenKey);
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        await localStorage.SetItemAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, refreshToken);
        await localStorage.SetItemAsync(ExpiryTimeKey, expiresAt.ToString("O"));
    }

    public async Task ClearTokensAsync()
    {
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(ExpiryTimeKey);
    }

    public async Task<bool> IsTokenExpiredAsync()
    {
        var expiryTime = await GetExpiryTimeAsync();
        return !expiryTime.HasValue || expiryTime.Value <= DateTime.UtcNow;
    }

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
