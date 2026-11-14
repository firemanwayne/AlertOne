namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to create an alert of a given type.
/// </summary>
public sealed record CreateAlertRequest(string AlertTypeId, string CreatedBy);
