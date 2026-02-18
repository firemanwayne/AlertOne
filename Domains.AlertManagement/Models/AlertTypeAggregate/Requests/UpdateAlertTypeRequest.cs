namespace Domains.AlertManagement.AlertTypeAggregate.Requests;

/// <summary>
/// Request model for updating an alert type
/// </summary>
public sealed record UpdateAlertTypeRequest(
    string Name,
    string Description,
    string Color,
    List<Guid> NotifyUserIds);
