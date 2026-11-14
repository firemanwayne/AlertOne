using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

/// <summary>
/// Links a user to an alert type so the user is notified for it.
/// </summary>
public class AlertTypeUser : DataModelBase
{
    /// <summary>
    /// Gets or sets the identifier of the alert type.
    /// </summary>
    public string AlertTypeId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the identifier of the user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    // EF Core navigation property
    /// <summary>
    /// Gets or sets the alert type (navigation property).
    /// </summary>
    public AlertType AlertType { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
