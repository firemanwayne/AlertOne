namespace Domains.AlertManagement.AlertTypeAggregate.Dtos;

/// <summary>
/// Response model for an alert type
/// </summary>
public sealed class AlertTypeResponse : DataModelBase
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
    /// Gets or sets the identifiers of the users notified for this alert type.
    /// </summary>
    public IReadOnlyList<string> NotifyUserIds { get; set; } = [];
}
