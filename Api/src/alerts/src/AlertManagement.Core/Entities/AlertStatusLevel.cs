using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

/// <summary>
/// A status level an alert can be in, with its display colors and notification behavior.
/// </summary>
public class AlertStatusLevel : DataModelBase
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    public string TextColor { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether notifications are sent when an alert reaches this level.
    /// </summary>
    public bool SendNotifications { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this is the default level for new alerts.
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the level is canceled.
    /// </summary>
    public bool IsCanceled { get; set; }
}
