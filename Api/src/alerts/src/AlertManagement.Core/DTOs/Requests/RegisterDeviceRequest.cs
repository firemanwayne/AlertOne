namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to register a device for push notifications.
/// </summary>
public sealed record RegisterDeviceRequest(
    string DeviceToken,
    string Platform,
    string UserId,
    IEnumerable<string>? Tags = null);
