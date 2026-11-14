namespace Domains.AlertManagement.AlertAggregate.Dtos;

/// <summary>
/// Response model for an alert
/// </summary>
public sealed class AlertResponse : DataModelBase
{
    /// <summary>
    /// Gets or sets the alert type.
    /// </summary>
    public AlertTypeResponse Type { get; set; } = new();

    /// <summary>
    /// Gets or sets the current status level.
    /// </summary>
    public AlertStatusLevelResponse StatusLevel { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the alert is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the UTC time the alert was rescinded.
    /// </summary>
    public DateTime? RescindedAt { get; set; }

    /// <summary>
    /// Gets or sets who rescinded the alert.
    /// </summary>
    public string? RescindedBy { get; set; }
}
