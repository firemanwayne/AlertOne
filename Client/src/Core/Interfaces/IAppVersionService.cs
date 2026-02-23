namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for retrieving application version information
/// </summary>
public interface IAppVersionService
{
    /// <summary>
    /// Gets the display version (e.g., "1.0.0")
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the build number (e.g., "123")
    /// </summary>
    string Build { get; }

    /// <summary>
    /// Gets the full version string (e.g., "1.0.0 (123)")
    /// </summary>
    string FullVersion { get; }
}
