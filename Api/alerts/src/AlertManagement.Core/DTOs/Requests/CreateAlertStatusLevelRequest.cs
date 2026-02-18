namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertStatusLevelRequest(string Name, string Description, string Color, bool SendNotifications);
