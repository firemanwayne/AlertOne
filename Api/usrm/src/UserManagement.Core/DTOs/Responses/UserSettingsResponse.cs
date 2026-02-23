namespace UserManagement.Core.DTOs.Responses;

public sealed record UserSettingsResponse(
    string Id,
    Guid UserId,
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme,
    DateTime CreatedDate,
    DateTime UpdatedDate);
