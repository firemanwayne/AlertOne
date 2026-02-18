using UserManagementApps.Core.Enums;

namespace UserManagementApps.Core.DTOs.Responses;

/// <summary>
/// Response model for device session information
/// </summary>
public sealed record DeviceSessionResponse(
    Guid DeviceId,
    string DeviceName,
    DeviceType DeviceType,
    string Platform,
    string? AppVersion,
    bool IsTrusted,
    bool IsCurrentDevice,
    string? IpAddress,
    string? Location,
    DateTime FirstLoginAt,
    DateTime LastLoginAt,
    DateTime? LastActivityAt,
    string? PushNotificationToken);
