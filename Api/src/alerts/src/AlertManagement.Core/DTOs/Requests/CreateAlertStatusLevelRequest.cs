namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertStatusLevelRequest(string Name, string Description, string BackgroundColor, string TextColor, bool SendNotifications, bool IsDefault, bool IsCanceled);
