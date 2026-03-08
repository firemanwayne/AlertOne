namespace AlertManagement.Core.DTOs.Requests;

public sealed record UpdateAlertStatusRequest(string AlertStatusLevelId, string UpdatedBy);
