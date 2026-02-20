using AlertOne.Core.Interfaces;
using AlertOne.Core.Models;
using AlertOne.Shared.States;

using Microsoft.AspNetCore.Components.Authorization;

using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AlertOne.Shared.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly AppState _appState;

    public JwtAuthenticationStateProvider(ITokenStorageService tokenStorage, AppState appState)
    {
        _tokenStorage = tokenStorage;
        _appState = appState;
        _appState.PropertyChanged += OnAppStatePropertyChanged;
    }

    private void OnAppStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppState.IsAuthenticated))
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token).ToList();

        // Hydrate AppState from JWT claims when missing (e.g. page refresh)
        if (_appState.CurrentUser?.Id == Guid.Empty || !_appState.IsAuthenticated)
        {
            var userId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdResult))
            {
                // Invalid token - return unauthenticated
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var email = claims
                .FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty;

            var firstName = claims
                .FirstOrDefault(c => c.Type == "firstName")?.Value ?? string.Empty;

            var lastName = claims
                .FirstOrDefault(c => c.Type == "lastName")?.Value ?? string.Empty;

            var roles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            _appState.PropertyChanged -= OnAppStatePropertyChanged;

            _appState.AccessToken = token;
            _appState.CurrentUser = new UserProfile
            {
                Id = userIdResult,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Roles = roles
            };
            _appState.IsAuthenticated = true;

            _appState.PropertyChanged += OnAppStatePropertyChanged;
        }

        var identity = new ClaimsIdentity(claims, "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static List<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            return [];
        }

        var payload = parts[1];
        var padding = (4 - payload.Length % 4) % 4;
        payload = payload.Replace('-', '+').Replace('_', '/') + new string('=', padding);

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch
        {
            return [];
        }

        var json = Encoding.UTF8.GetString(bytes);

        Dictionary<string, JsonElement>? keyValuePairs;
        try
        {
            keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        }
        catch
        {
            return [];
        }

        if (keyValuePairs is null)
        {
            return [];
        }

        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in kvp.Value.EnumerateArray())
                {
                    claims.Add(new Claim(kvp.Key, element.GetString() ?? string.Empty));
                }
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }
        }

        return claims;
    }

    public void Dispose()
    {
        _appState.PropertyChanged -= OnAppStatePropertyChanged;
    }
}
