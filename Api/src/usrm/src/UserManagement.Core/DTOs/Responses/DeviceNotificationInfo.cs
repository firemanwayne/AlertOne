using UserManagement.Core.Enums;

namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Device information for notification services
/// </summary>
public sealed record DeviceNotificationInfo(
    Guid DeviceId,
    DeviceType DeviceType,
    string Platform,
    string? PushNotificationToken,
    bool IsActive);
