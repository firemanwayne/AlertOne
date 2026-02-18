namespace Domains.AlertManagement.AlertAggregate.Requests;

/// <summary>
/// Request to rescind an active alert
/// </summary>
public sealed record RescindAlertRequest(string RescindedBy);
