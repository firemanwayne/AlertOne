namespace Domains.AlertManagement.AlertAggregate.Dtos;

/// <summary>
/// Response model for an alert
/// </summary>
public sealed class AlertResponse : DataModelBase
{
    public AlertTypeResponse Type { get; set; } = new();

    public AlertStatusLevelResponse StatusLevel { get; set; } = new();

    public bool IsActive { get; set; }

    public DateTime? RescindedAt { get; set; }

    public string? RescindedBy { get; set; }
}
