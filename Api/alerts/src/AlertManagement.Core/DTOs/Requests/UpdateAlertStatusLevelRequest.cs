namespace AlertManagement.Core.DTOs.Requests;

public sealed record UpdateAlertStatusLevelRequest(string Name, string Description, string Color, bool SendNotifications);
