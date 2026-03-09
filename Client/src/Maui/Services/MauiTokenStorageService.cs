
using AlertOne.Core.Interfaces;

namespace AlertOne.Maui.Services;

/// <summary>
/// Token storage service using MAUI SecureStorage
/// </summary>
internal sealed class MauiTokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiryTimeKey = "token_expiry";

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(AccessTokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(RefreshTokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<DateTime?> GetExpiryTimeAsync()
    {
        try
        {
            var expiryString = await SecureStorage.GetAsync(ExpiryTimeKey);

            if (string.IsNullOrEmpty(expiryString))
            {
                return null;
            }

            if (DateTime.TryParse(expiryString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
            {
                // Ensure we return UTC for proper comparison
                return expiry.Kind == DateTimeKind.Utc
                    ? expiry
                    : expiry.ToUniversalTime();
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        try
        {
            await SecureStorage.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
            await SecureStorage.SetAsync(ExpiryTimeKey, expiresAt.ToString("O"));
        }
        catch (Exception)
        {
            // Log error in production
        }
    }

    public async Task ClearTokensAsync()
    {
        try
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(ExpiryTimeKey);
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Log error in production
        }
    }

    public async Task<bool> IsTokenExpiredAsync()
    {
        var expiryTime = await GetExpiryTimeAsync();

        if (expiryTime == null)
        {
            return true;
        }

        return DateTime.UtcNow >= expiryTime.Value;
    }
}
