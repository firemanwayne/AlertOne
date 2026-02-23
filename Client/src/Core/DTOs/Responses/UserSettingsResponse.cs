namespace AlertOne.Core.DTOs.Responses;

/// <summary>
/// Response model for user settings
/// </summary>
public sealed record UserSettingsResponse(
    string Id,
    Guid UserId,
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme,
    DateTime CreatedDate,
    DateTime UpdatedDate);
