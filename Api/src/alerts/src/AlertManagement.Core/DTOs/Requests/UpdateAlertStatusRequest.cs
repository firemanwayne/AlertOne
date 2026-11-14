namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to move an alert to a different status level.
/// </summary>
public sealed record UpdateAlertStatusRequest(string AlertStatusLevelId, string UpdatedBy);
