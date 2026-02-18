namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertRequest(Guid AlertTypeId, Guid AlertStatusLevelId, string CreatedBy);
