using AlertOne.Maui.Services;
using Foundation;
using UIKit;

namespace AlertOne.Maui.Platforms.iOS;

/// <summary>
/// The iOS application delegate.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>
    /// Creates the MAUI app.
    /// </summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /// <summary>
    /// Handles a custom scheme URL by passing it to the deep link service.
    /// </summary>
    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        HandleDeepLink(url.ToString(), isAppRunning: true);
        return true;
    }

    /// <summary>
    /// Handles a universal link by passing it to the deep link service.
    /// </summary>
    public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
    {
        // Handle Universal Links
        if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb && userActivity.WebPageUrl is not null)
        {
            HandleDeepLink(userActivity.WebPageUrl.ToString(), isAppRunning: true);
            return true;
        }

        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }

    /// <summary>
    /// Passes a deep link URL to the deep link service.
    /// </summary>
    private static void HandleDeepLink(string? url, bool isAppRunning)
    {
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var deepLinkService = IPlatformApplication.Current?.Services.GetService<DeepLinkService>();
        deepLinkService?.HandleDeepLink(url, isAppRunning);
    }
}
