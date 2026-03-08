using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

public class Alert : DataModelBase
{
    public string AlertTypeId { get; set; } = string.Empty;
    public string AlertStatusLevelId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? RescindedAt { get; set; }
    public string? RescindedBy { get; set; }

    // EF Core navigation properties
    public AlertType AlertType { get; set; } = null!;
    public AlertStatusLevel AlertStatusLevel { get; set; } = null!;
}
