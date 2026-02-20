#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertAggregate.Requests;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Request to rescind an active alert
/// </summary>
public sealed record RescindAlertRequest(string RescindedBy);
