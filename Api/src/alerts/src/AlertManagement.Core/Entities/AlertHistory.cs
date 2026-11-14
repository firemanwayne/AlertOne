using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

/// <summary>
/// A recorded event in the life of an alert (created, status updated, rescinded).
/// </summary>
public class AlertHistory : DataModelBase
{
    /// <summary>
    /// Gets or sets the identifier of the alert.
    /// </summary>
    public string AlertId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets optional notes for the event.
    /// </summary>
    public string? Notes { get; set; }
    /// <summary>
    /// Gets or sets the alert (navigation property).
    /// </summary>
    public Alert Alert { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
