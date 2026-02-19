namespace AlertOne.Core.Models;

/// <summary>
/// Represents the current authentication state
/// </summary>
public sealed class AuthState
{
    /// <summary>
    /// Gets or sets a value indicating whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the current user profile
    /// </summary>
    public UserProfile? CurrentUser { get; set; }

    /// <summary>
    /// Gets or sets the current access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Clears the authentication state
    /// </summary>
    public void Clear()
    {
        IsAuthenticated = false;
        CurrentUser = null;
        AccessToken = null;
        ExpiresAt = null;
    }
}
