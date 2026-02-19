#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertAggregate.Requests;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Request model for creating a new alert
/// </summary>
public sealed record CreateAlertRequest(
    string AlertTypeId,
    string AlertStatusLevelId,
    string CreatedBy);
