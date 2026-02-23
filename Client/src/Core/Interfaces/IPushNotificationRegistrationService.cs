namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for registering and managing push notification subscriptions on the client device.
/// </summary>
public interface IPushNotificationRegistrationService
{
    /// <summary>
    /// Gets a value indicating whether push notifications are supported on this platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether the device is currently registered for push notifications.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Gets the current device token if registered.
    /// </summary>
    string? DeviceToken { get; }

    /// <summary>
    /// Gets the registration ID from the notification hub if registered.
    /// </summary>
    string? RegistrationId { get; }

    /// <summary>
    /// Initializes the push notification service and requests necessary permissions.
    /// </summary>
    /// <returns>True if initialization was successful.</returns>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Registers the current device for push notifications.
    /// </summary>
    /// <param name="userId">The user ID to associate with this device.</param>
    /// <param name="tags">Optional additional tags for targeting notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration ID if successful, null otherwise.</returns>
    Task<string?> RegisterAsync(string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters the current device from push notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if unregistration was successful.</returns>
    Task<bool> UnregisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when a push notification is received.
    /// </summary>
    event EventHandler<PushNotificationReceivedEventArgs>? NotificationReceived;

    /// <summary>
    /// Event raised when the device token is refreshed.
    /// </summary>
    event EventHandler<string>? TokenRefreshed;
}

/// <summary>
/// Event arguments for when a push notification is received.
/// </summary>
public class PushNotificationReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the notification title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the notification body.
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Gets the notification data payload.
    /// </summary>
    public IDictionary<string, string>? Data { get; init; }

    /// <summary>
    /// Gets a value indicating whether the notification was received while the app was in the foreground.
    /// </summary>
    public bool ReceivedInForeground { get; init; }
}
