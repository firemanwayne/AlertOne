namespace AlertOne.Core.DTOs.Responses;

/// <summary>
/// Response from registering a device for push notifications.
/// </summary>
public sealed class DeviceRegistrationResponse
{
    /// <summary>
    /// The registration ID from Azure Notification Hubs.
    /// </summary>
    public required string RegistrationId { get; init; }

    /// <summary>
    /// The platform the device was registered for.
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// The timestamp when the registration was created or updated.
    /// </summary>
    public DateTime RegisteredAt { get; init; }
}
