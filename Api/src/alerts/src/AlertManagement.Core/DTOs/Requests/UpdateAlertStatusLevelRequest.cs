namespace AlertManagement.Core.DTOs.Requests;

public sealed record UpdateAlertStatusLevelRequest(string Name, string Description, string BackgroundColor, string TextColor, bool SendNotifications, bool IsDefault, bool IsCanceled);
