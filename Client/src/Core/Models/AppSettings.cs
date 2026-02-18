namespace UserManagementApps.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Gets or sets the API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7071";

    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    public string AppVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
}
