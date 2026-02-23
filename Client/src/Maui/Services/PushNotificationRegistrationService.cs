using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.DTOs.Responses;
using AlertOne.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace AlertOne.Maui.Services;

/// <summary>
/// MAUI implementation of push notification registration service.
/// </summary>
public sealed class PushNotificationRegistrationService(
    IAlertManagementApiClient apiClient,
    ILogger<PushNotificationRegistrationService> logger) : IPushNotificationRegistrationService
{
    private string? _deviceToken;
    private string? _registrationId;
    private bool _isInitialized;

    public bool IsSupported =>
#if ANDROID
        true;
#elif IOS
        true;
#else
        false;
#endif

    public bool IsRegistered => !string.IsNullOrEmpty(_registrationId);

    public string? DeviceToken => _deviceToken;

    public string? RegistrationId => _registrationId;

    public event EventHandler<PushNotificationReceivedEventArgs>? NotificationReceived;

    public event EventHandler<string>? TokenRefreshed;

    public async Task<bool> InitializeAsync()
    {
        if (_isInitialized)
        {
            return true;
        }

        try
        {
#if ANDROID
            _isInitialized = await InitializeAndroidAsync();
#elif IOS
            _isInitialized = await InitializeIosAsync();
#else
            logger.LogWarning("Push notifications not supported on this platform");
            _isInitialized = false;
#endif
            return _isInitialized;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize push notifications");
            return false;
        }
    }

    public async Task<string?> RegisterAsync(string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            var initialized = await InitializeAsync();

            if (!initialized)
            {
                logger.LogWarning("Cannot register: Push notifications not initialized");
                return null;
            }
        }

        if (string.IsNullOrEmpty(_deviceToken))
        {
            logger.LogWarning("Cannot register: No device token available");
            return null;
        }

        try
        {
            var platform = GetPlatformIdentifier();
            var request = new RegisterDeviceRequest
            {
                DeviceToken = _deviceToken,
                Platform = platform,
                UserId = userId,
                Tags = tags
            };

            logger.LogInformation("Registering device for push notifications. Platform: {Platform}, UserId: {UserId}", platform, userId);

            var result = await apiClient.PostAsync<RegisterDeviceRequest, DeviceRegistrationResponse>("/api/notifications/register", request, cancellationToken);

            if (result.IsSuccess)
            {
                _registrationId = result.Value.RegistrationId;
                logger.LogInformation("Device registered successfully. RegistrationId: {RegistrationId}", _registrationId);
                return _registrationId;
            }

            logger.LogWarning("Failed to register device: {Error}", result.Error?.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register device for push notifications");
            return null;
        }
    }

    public async Task<bool> UnregisterAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_registrationId))
        {
            logger.LogWarning("Cannot unregister: No registration ID available");
            return true;
        }

        try
        {
            logger.LogInformation("Unregistering device. RegistrationId: {RegistrationId}", _registrationId);

            var result = await apiClient.DeleteAsync($"/api/notifications/unregister/{_registrationId}", cancellationToken);

            if (result.IsSuccess)
            {
                _registrationId = null;
                logger.LogInformation("Device unregistered successfully");
                return true;
            }

            logger.LogWarning("Failed to unregister device: {Error}", result.Error?.Message);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unregister device");
            return false;
        }
    }

    internal void OnTokenReceived(string token)
    {
        logger.LogInformation("FCM token received");
        _deviceToken = token;
        TokenRefreshed?.Invoke(this, token);
    }

    internal void OnNotificationReceived(string? title, string? body, IDictionary<string, string>? data, bool inForeground)
    {
        logger.LogInformation("Push notification received. Title: {Title}, InForeground: {InForeground}", title, inForeground);

        NotificationReceived?.Invoke(this, new PushNotificationReceivedEventArgs
        {
            Title = title,
            Body = body,
            Data = data,
            ReceivedInForeground = inForeground
        });
    }

    private static string GetPlatformIdentifier()
    {
#if ANDROID
        return "fcm";
#elif IOS
        return "apns";
#elif WINDOWS
        return "wns";
#else
        return "unknown";
#endif
    }

#if ANDROID
    private Task<bool> InitializeAndroidAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            if (Android.Gms.Common.GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Android.App.Application.Context) != Android.Gms.Common.ConnectionResult.Success)
            {
                logger.LogWarning("Google Play Services not available");
                tcs.SetResult(false);
                return tcs.Task;
            }

            Firebase.Messaging.FirebaseMessaging.Instance.GetToken()
                .AddOnCompleteListener(new TokenCompleteListener(token =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        _deviceToken = token;
                        logger.LogInformation("FCM token obtained successfully");
                    }

                    tcs.SetResult(true);
                }, ex =>
                {
                    logger.LogError(ex, "Failed to get FCM token");
                    tcs.SetResult(false);
                }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Firebase on Android");
            tcs.SetResult(false);
        }

        return tcs.Task;
    }

    private sealed class TokenCompleteListener(Action<string?> onSuccess, Action<Exception> onFailure) : Java.Lang.Object, Android.Gms.Tasks.IOnCompleteListener
    {
        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful)
            {
                onSuccess(task.Result?.ToString());
            }
            else
            {
                onFailure(task.Exception ?? new Exception("Unknown error getting FCM token"));
            }
        }
    }
#endif

#if IOS
    private async Task<bool> InitializeIosAsync()
    {
        try
        {
            var authStatus = await UserNotifications.UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                UserNotifications.UNAuthorizationOptions.Alert |
                UserNotifications.UNAuthorizationOptions.Badge |
                UserNotifications.UNAuthorizationOptions.Sound);

            if (!authStatus.Item1)
            {
                logger.LogWarning("Push notification authorization denied");
                return false;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UIKit.UIApplication.SharedApplication.RegisterForRemoteNotifications();
            });

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize push notifications on iOS");
            return false;
        }
    }
#endif
}
