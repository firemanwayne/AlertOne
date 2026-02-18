namespace Domains.AlertManagement.AlertAggregate.Requests;

public sealed record UpdateAlertStatusRequest(Guid AlertStatusLevelId, string UpdatedBy);
