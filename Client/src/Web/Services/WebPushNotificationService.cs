using AlertOne.Core.Interfaces;

namespace AlertOne.Web.Services;

/// <summary>
/// Stub implementation of push notification service for WebAssembly.
/// Web push notifications are not currently supported.
/// </summary>
internal sealed class WebPushNotificationService : IPushNotificationRegistrationService
{
    public bool IsSupported => false;

    public bool IsRegistered => false;

    public string? DeviceToken => null;

    public string? RegistrationId => null;

    public event EventHandler<PushNotificationReceivedEventArgs>? NotificationReceived;

    public event EventHandler<string>? TokenRefreshed;

    public Task<bool> InitializeAsync()
    {
        return Task.FromResult(false);
    }

    public Task<string?> RegisterAsync(string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<bool> UnregisterAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
