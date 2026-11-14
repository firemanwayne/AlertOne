namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to rescind an alert.
/// </summary>
public sealed record RescindAlertRequest(string RescindedBy);
