namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request to update a user's settings.
/// </summary>
public sealed record UpdateUserSettingsRequest(
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme);
