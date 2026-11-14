namespace Domains.AlertManagement.AlertAggregate.Dtos;

/// <summary>
/// One entry in the history of an alert.
/// </summary>
public sealed class AlertHistoryResponse
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the alert id.
    /// </summary>
    public string AlertId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the changed by.
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the changed at.
    /// </summary>
    public DateTime ChangedAt { get; set; }
    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
}
