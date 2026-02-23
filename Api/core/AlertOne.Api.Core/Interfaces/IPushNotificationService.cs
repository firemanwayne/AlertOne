namespace AlertOne.Api.Core.Interfaces;

/// <summary>
/// Interface for sending push notifications via Azure Notification Hubs.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Registers a device for push notifications.
    /// </summary>
    /// <param name="deviceToken">The device token (FCM token for Android, APNs token for iOS).</param>
    /// <param name="platform">The platform (fcm, apns, wns).</param>
    /// <param name="userId">The user ID to tag the registration with.</param>
    /// <param name="tags">Additional tags for targeting notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration ID.</returns>
    Task<string> RegisterDeviceAsync(string deviceToken, string platform, string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a device from push notifications.
    /// </summary>
    /// <param name="registrationId">The registration ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnregisterDeviceAsync(string registrationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to a specific user.
    /// </summary>
    /// <param name="userId">The user ID to send the notification to.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="data">Additional data payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the notification was sent successfully.</returns>
    Task<bool> SendToUserAsync(string userId, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to users with specific tags.
    /// </summary>
    /// <param name="tags">The tags to target (e.g., "role:admin", "team:sales").</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="data">Additional data payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the notification was sent successfully.</returns>
    Task<bool> SendToTagsAsync(IEnumerable<string> tags, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to all registered devices.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="data">Additional data payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the notification was sent successfully.</returns>
    Task<bool> SendToAllAsync(string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}
