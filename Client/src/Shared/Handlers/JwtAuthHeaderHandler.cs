using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.Interfaces;
using AlertOne.Shared.States;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace AlertOne.Shared.Handlers;

/// <summary>
/// HTTP message handler that adds JWT authentication headers to requests
/// </summary>
public sealed class JwtAuthHeaderHandler(
    ITokenStorageService tokenStorage,
    IServiceProvider services,
    AppState appState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip auth for login/register/refresh endpoints
        if (request.RequestUri?.AbsolutePath.Contains("/auth/") == true)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception (logging mechanism not shown here)
                Console.WriteLine($"Error during auth request: {ex.Message}");
                throw;
            }
        }

        // Check if token is expired or about to expire (within 5 minutes)
        var isExpired = await tokenStorage.IsTokenExpiredAsync();
        var expiryTime = await tokenStorage.GetExpiryTimeAsync();
        var isAboutToExpire = expiryTime.HasValue && expiryTime.Value.Subtract(DateTime.UtcNow).TotalMinutes < 5;

        if (isExpired || isAboutToExpire)
        {
            // Attempt to refresh the token
            var refreshToken = await tokenStorage.GetRefreshTokenAsync();

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshRequest = new RefreshTokenRequest(refreshToken);
                var authService = services.GetRequiredService<IAuthenticationService>();
                var refreshResult = await authService.RefreshTokenAsync(refreshRequest, cancellationToken);

                if (refreshResult.IsSuccess)
                {
                    // Token refreshed successfully, update storage
                    var authResponse = refreshResult.Value;
                    await tokenStorage.SaveTokensAsync(
                        authResponse.AccessToken,
                        authResponse.RefreshToken,
                        authResponse.ExpiresAt);

                    appState.AccessToken = authResponse.AccessToken;
                }
                else
                {
                    // Refresh failed, clear tokens and return unauthorized
                    await tokenStorage.ClearTokensAsync();
                    appState.Clear();

                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent("Token refresh failed")
                    };
                    return response;
                }
            }
        }

        // Add Authorization header
        var accessToken = await tokenStorage.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // Send the request
        var httpResponse = await base.SendAsync(request, cancellationToken);

        // Handle 401 Unauthorized responses
        if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            await tokenStorage.ClearTokensAsync();
            appState.Clear();
        }

        return httpResponse;
    }
}
