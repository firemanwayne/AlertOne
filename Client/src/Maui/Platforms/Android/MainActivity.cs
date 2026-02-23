using AlertOne.Maui.Platforms.Android.Services;
using AlertOne.Maui.Services;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using AndroidX.Core.App;
using AndroidX.Core.Content;

using Firebase;

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

#if !ANDROID24_0_OR_GREATER
        // Set status bar color to match app header
        Window?.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#1a2c4e"));
#endif
        // Initialize Firebase
        InitializeFirebase();

        // Request notification permission for Android 13+
        RequestNotificationPermission();

        // Handle deep link on app launch
        HandleDeepLinkIntent(Intent, isAppRunning: false);
    }

    private void RequestNotificationPermission()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, [Manifest.Permission.PostNotifications], 0);
            }
        }
    }

    private void InitializeFirebase()
    {
        try
        {
            if (FirebaseApp.GetApps(this).Count == 0)
            {
                FirebaseApp.InitializeApp(this);
            }

            // Set the static reference for the Firebase messaging service
            var pushService = IPlatformApplication.Current?.Services.GetService<PushNotificationRegistrationService>();

            if (pushService is not null)
            {
                AlertOneFirebaseMessagingService.PushNotificationService = pushService;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize Firebase: {ex.Message}");
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        // Handle deep link when app is already running
        HandleDeepLinkIntent(intent, isAppRunning: true);
    }

#pragma warning disable CA1822 // Mark members as static
    private void HandleDeepLinkIntent(Intent? intent, bool isAppRunning)
#pragma warning restore CA1822 // Mark members as static
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
