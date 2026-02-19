#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertAggregate.Requests;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed record UpdateAlertStatusRequest(string AlertStatusLevelId, string UpdatedBy);
