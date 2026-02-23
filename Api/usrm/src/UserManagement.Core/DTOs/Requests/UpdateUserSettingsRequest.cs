namespace UserManagement.Core.DTOs.Requests;

public sealed record UpdateUserSettingsRequest(
    bool ReceiveEmailNotifications,
    bool ReceivePushNotifications,
    string? PreferredTheme);
