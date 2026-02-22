using AlertOne.Maui.Services;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace AlertOne.Maui.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionView],
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "alertone")]
[IntentFilter(
    [Intent.ActionView],
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "app.alertone.com",
    AutoVerify = true)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set status bar color to match app header
        if (Window != null)
        {
            Window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#1a2c4e"));
        }

        // Handle deep link on app launch
        HandleDeepLinkIntent(Intent, isAppRunning: false);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        // Handle deep link when app is already running
        HandleDeepLinkIntent(intent, isAppRunning: true);
    }

    private void HandleDeepLinkIntent(Intent? intent, bool isAppRunning)
    {
        if (intent?.Data is null)
        {
            return;
        }

        var url = intent.Data.ToString();
        var deepLinkService = IPlatformApplication.Current?.Services.GetService<DeepLinkService>();
        deepLinkService?.HandleDeepLink(url, isAppRunning);
    }
}
