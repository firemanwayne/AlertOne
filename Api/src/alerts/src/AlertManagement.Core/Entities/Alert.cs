using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

/// <summary>
/// An alert raised for an alert type, moving through status levels until rescinded.
/// </summary>
public class Alert : DataModelBase
{
    /// <summary>
    /// Gets or sets the identifier of the alert type.
    /// </summary>
    public string AlertTypeId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the identifier of the current status level.
    /// </summary>
    public string AlertStatusLevelId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the alert is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the UTC time the alert was rescinded.
    /// </summary>
    public DateTime? RescindedAt { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user that rescinded the alert.
    /// </summary>
    public string? RescindedBy { get; set; }

    // EF Core navigation properties
    /// <summary>
    /// Gets or sets the alert type (navigation property).
    /// </summary>
    public AlertType AlertType { get; set; } = null!;
    /// <summary>
    /// Gets or sets the current status level (navigation property).
    /// </summary>
    public AlertStatusLevel AlertStatusLevel { get; set; } = null!;
}
