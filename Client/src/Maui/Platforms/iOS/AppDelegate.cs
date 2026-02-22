using AlertOne.Maui.Services;
using Foundation;
using UIKit;

namespace AlertOne.Maui.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        HandleDeepLink(url.ToString(), isAppRunning: true);
        return true;
    }

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
