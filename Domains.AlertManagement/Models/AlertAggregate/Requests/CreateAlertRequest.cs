namespace Domains.AlertManagement.AlertAggregate.Requests;

/// <summary>
/// Request model for creating a new alert
/// </summary>
public sealed record CreateAlertRequest(
    Guid AlertTypeId,
    Guid AlertStatusLevelId,
    string CreatedBy);
