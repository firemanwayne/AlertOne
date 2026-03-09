namespace Domains.AlertManagement.AlertStatusLevelAggregate.Requests;

/// <summary>
/// Request model for updating an alert status level
/// </summary>
public sealed record UpdateAlertStatusLevelRequest(
    string Name,
    string Description,
    string BackgroundColor,
    string TextColor,
    bool SendNotifications,
    bool IsDefault,
    bool IsCanceled);
