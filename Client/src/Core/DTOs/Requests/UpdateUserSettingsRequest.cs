namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request model for updating user settings
/// </summary>
public sealed record UpdateUserSettingsRequest(
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme);
