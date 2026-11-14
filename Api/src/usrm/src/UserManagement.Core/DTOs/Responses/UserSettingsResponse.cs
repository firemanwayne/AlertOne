namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// A user's settings.
/// </summary>
public sealed record UserSettingsResponse(
    string Id,
    Guid UserId,
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme,
    DateTime CreatedDate,
    DateTime UpdatedDate);
