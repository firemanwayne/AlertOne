namespace UserManagement.Infrastructure.Configurations;

/// <summary>
/// Configuration settings for JWT token generation
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Gets or sets the key used to sign access tokens.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets how long an access token is valid, in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;
}
