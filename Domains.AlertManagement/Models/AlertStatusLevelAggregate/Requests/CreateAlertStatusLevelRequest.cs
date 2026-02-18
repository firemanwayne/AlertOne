namespace Domains.AlertManagement.AlertStatusLevelAggregate.Requests;

/// <summary>
/// Request model for creating a new alert status level
/// </summary>
public sealed record CreateAlertStatusLevelRequest(
    string Name,
    string Description,
    string Color,
    bool SendNotifications);
