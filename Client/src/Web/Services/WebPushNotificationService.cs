using AlertOne.Core.Interfaces;

namespace AlertOne.Web.Services;

/// <summary>
/// Stub implementation of push notification service for WebAssembly.
/// Web push notifications are not currently supported.
/// </summary>
internal sealed class WebPushNotificationService : IPushNotificationRegistrationService
{
    /// <inheritdoc />
    public bool IsSupported => false;

    /// <inheritdoc />
    public bool IsRegistered => false;

    /// <inheritdoc />
    public string? DeviceToken => null;

    /// <inheritdoc />
    public string? RegistrationId => null;

    /// <inheritdoc />
    public event EventHandler<PushNotificationReceivedEventArgs>? NotificationReceived;

    /// <inheritdoc />
    public event EventHandler<string>? TokenRefreshed;

    /// <inheritdoc />
    public Task<bool> InitializeAsync()
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<string?> RegisterAsync(string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task<bool> UnregisterAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
