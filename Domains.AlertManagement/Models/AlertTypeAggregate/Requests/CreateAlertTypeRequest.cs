namespace Domains.AlertManagement.AlertTypeAggregate.Requests;

/// <summary>
/// Request model for creating a new alert type
/// </summary>
public sealed record CreateAlertTypeRequest(
    string Name,
    string Description,
    string Color,
    List<string> NotifyUserIds);
