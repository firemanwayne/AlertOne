using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace AlertOne.Maui.Platforms.Android.Services;

/// <summary>
/// Firebase Cloud Messaging service for handling push notifications on Android.
/// </summary>
[Service(Exported = true)]
[IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
public class AlertOneFirebaseMessagingService : FirebaseMessagingService
{
    /// <summary>
    /// Static reference to the push notification service for forwarding events.
    /// </summary>
    public static AlertOne.Maui.Services.PushNotificationRegistrationService? PushNotificationService { get; set; }

    /// <summary>
    /// Called when a new FCM token is generated.
    /// </summary>
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);

        System.Diagnostics.Debug.WriteLine($"FCM Token refreshed: {token}");

        PushNotificationService?.OnTokenReceived(token);
    }

    /// <summary>
    /// Called when a message is received from FCM.
    /// </summary>
    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var notification = message.GetNotification();
        var data = message.Data;

        var title = notification?.Title;
        var body = notification?.Body;
        var dataDict = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        System.Diagnostics.Debug.WriteLine($"FCM Message received. Title: {title}, Body: {body}");

        // Determine if app is in foreground
        var isInForeground = IsAppInForeground();

        // Forward to the registration service
        PushNotificationService?.OnNotificationReceived(title, body, dataDict, isInForeground);

        // Show notification if app is in background
        if (!isInForeground && !string.IsNullOrEmpty(title))
        {
            ShowNotification(title, body ?? string.Empty, dataDict);
        }
    }

    private static bool IsAppInForeground()
    {
        var context = global::Android.App.Application.Context;
        var activityManager = (ActivityManager?)context.GetSystemService(ActivityService);

        if (activityManager is null)
        {
            return false;
        }

        var appProcesses = activityManager.RunningAppProcesses;

        if (appProcesses is null)
        {
            return false;
        }

        var packageName = context.PackageName;

        foreach (var process in appProcesses)
        {
            if (process.Importance == Importance.Foreground && process.ProcessName == packageName)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowNotification(string title, string body, IDictionary<string, string>? data)
    {
        var channelId = "alertone_notifications";
        var channelName = "AlertOne Notifications";

        var notificationManager = (NotificationManager?)GetSystemService(NotificationService);

        if (notificationManager is null)
        {
            return;
        }

        // Create notification channel for Android 8.0+
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
            {
                Description = "AlertOne push notifications"
            };

            notificationManager.CreateNotificationChannel(channel);
        }

        var intent = PackageManager?.GetLaunchIntentForPackage(PackageName ?? string.Empty);

        if (intent is not null && data is not null)
        {
            foreach (var kvp in data)
            {
                intent.PutExtra(kvp.Key, kvp.Value);
            }
        }

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetActivity(this, 0, intent, pendingIntentFlags);

        // Use Android's built-in notification icon
        var iconResourceId = global::Android.App.Application.Context.ApplicationInfo?.Icon ?? global::Android.Resource.Drawable.IcDialogInfo;

        var notificationBuilder = new Notification.Builder(this, channelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(iconResourceId)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent);

        var notificationId = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue);
        notificationManager.Notify(notificationId, notificationBuilder.Build());
    }
}
