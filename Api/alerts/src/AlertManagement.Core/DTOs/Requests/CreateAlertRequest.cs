namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertRequest(string AlertTypeId, string AlertStatusLevelId, string CreatedBy);
