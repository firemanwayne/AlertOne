namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to create an alert status level.
/// </summary>
public sealed record CreateAlertStatusLevelRequest(string Name, string Description, string BackgroundColor, string TextColor, bool SendNotifications, bool IsDefault, bool IsCanceled);
