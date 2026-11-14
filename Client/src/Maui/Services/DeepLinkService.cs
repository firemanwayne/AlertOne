namespace AlertOne.Maui.Services;

/// <summary>
/// Service for handling deep link URLs and providing navigation paths.
/// </summary>
public sealed class DeepLinkService
{
    /// <summary>
    /// The deep link received before the app was ready to navigate.
    /// </summary>
    private string? _pendingDeepLink;
    /// <summary>
    /// Synchronizes access to the pending deep link.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when a deep link is received while the app is running.
    /// </summary>
    public event EventHandler<string>? DeepLinkReceived;

    /// <summary>
    /// Gets or sets the pending deep link path to navigate to on startup.
    /// </summary>
    public string? PendingDeepLink
    {
        get
        {
            lock (_lock)
            {
                return _pendingDeepLink;
            }
        }
        set
        {
            lock (_lock)
            {
                _pendingDeepLink = value;
            }
        }
    }

    /// <summary>
    /// Clears and returns the pending deep link.
    /// </summary>
    public string? ConsumePendingDeepLink()
    {
        lock (_lock)
        {
            var link = _pendingDeepLink;
            _pendingDeepLink = null;
            return link;
        }
    }

    /// <summary>
    /// Processes a deep link URL and returns the Blazor navigation path.
    /// </summary>
    /// <param name="url">The deep link URL (e.g., alertone://accept-invitation?code=123456)</param>
    /// <returns>The Blazor navigation path, or null if the URL is invalid.</returns>
    public string? ProcessDeepLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);

            // Handle alertone:// scheme
            if (uri.Scheme.Equals("alertone", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessAlertOneScheme(uri);
            }

            // Handle https:// links (App Links / Universal Links)
            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessHttpsScheme(uri);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Handles a deep link by either storing it for startup or raising an event.
    /// </summary>
    /// <param name="url">The deep link URL.</param>
    /// <param name="isAppRunning">Whether the app is already running.</param>
    public void HandleDeepLink(string? url, bool isAppRunning)
    {
        var path = ProcessDeepLink(url);

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (isAppRunning)
        {
            DeepLinkReceived?.Invoke(this, path);
        }
        else
        {
            PendingDeepLink = path;
        }
    }

    /// <summary>
    /// Converts an <c>alertone://</c> link into an app route, or null when it is not recognized.
    /// </summary>
    private static string? ProcessAlertOneScheme(Uri uri)
    {
        // alertone://accept-invitation?code=123456
        // Host will be "accept-invitation" for custom scheme
        var path = uri.Host;
        var query = uri.Query;

        return path switch
        {
            "accept-invitation" => $"/accept-invitation{query}",
            "alerts" => "/alerts",
            "dashboard" => "/",
            _ => null
        };
    }

    /// <summary>
    /// Converts an <c>https://</c> link into an app route.
    /// </summary>
    private static string? ProcessHttpsScheme(Uri uri)
    {
        // https://app.alertone.com/accept-invitation?code=123456
        var path = uri.AbsolutePath;
        var query = uri.Query;

        if (path.StartsWith("/accept-invitation", StringComparison.OrdinalIgnoreCase))
        {
            return $"/accept-invitation{query}";
        }

        if (path.StartsWith("/alerts", StringComparison.OrdinalIgnoreCase))
        {
            return "/alerts";
        }

        return path;
    }
}
