namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Response when registering or updating a device
/// </summary>
public sealed record RegisterDeviceResponse(
    string RefreshToken,
    DeviceSessionResponse Device);
