using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

/// <summary>
/// A kind of alert, with its display colors and the users that are notified.
/// </summary>
public class AlertType : DataModelBase
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

    // EF Core navigation property
    /// <summary>
    /// Gets or sets the users notified for this alert type (navigation property).
    /// </summary>
    public ICollection<AlertTypeUser> AlertTypeUsers { get; set; } = null!;
}
