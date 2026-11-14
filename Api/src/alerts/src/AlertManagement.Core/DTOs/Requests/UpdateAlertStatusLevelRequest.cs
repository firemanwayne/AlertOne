namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to update an alert status level.
/// </summary>
public sealed record UpdateAlertStatusLevelRequest(string Name, string Description, string BackgroundColor, string TextColor, bool SendNotifications, bool IsDefault, bool IsCanceled);
