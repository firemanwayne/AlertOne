namespace AlertManagement.Core.DTOs.Responses;

/// <summary>
/// Response from registering a device for push notifications.
/// </summary>
public sealed record DeviceRegistrationResponse(
    string RegistrationId,
    string Platform,
    DateTime RegisteredAt);
