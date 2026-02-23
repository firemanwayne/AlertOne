namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request to register a device for push notifications.
/// </summary>
public sealed class RegisterDeviceRequest
{
    /// <summary>
    /// The device token from FCM (Android) or APNs (iOS).
    /// </summary>
    public required string DeviceToken { get; init; }

    /// <summary>
    /// The platform: "fcm" for Android, "apns" for iOS, "wns" for Windows.
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// The user ID to associate with this device registration.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Optional additional tags for targeting notifications.
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }
}
