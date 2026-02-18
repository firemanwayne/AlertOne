using UserManagementApps.Core.Abstractions;

namespace Domains.AlertManagement.AlertStatusLevelAggregate.Dtos;

/// <summary>
/// Response model for an alert status level
/// </summary>
public sealed class AlertStatusLevelResponse : DataModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public bool SendNotifications { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }
}
