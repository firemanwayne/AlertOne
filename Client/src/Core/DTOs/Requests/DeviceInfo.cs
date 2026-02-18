using UserManagementApps.Core.Enums;

namespace UserManagementApps.Core.DTOs.Requests;

/// <summary>
/// Information about the device making the authentication request
/// </summary>
public sealed record DeviceInfo(
    string DeviceId,
    string DeviceName,
    DeviceType DeviceType,
    string Platform,
    string? AppVersion,
    string? UserAgent,
    string? PushNotificationToken);
