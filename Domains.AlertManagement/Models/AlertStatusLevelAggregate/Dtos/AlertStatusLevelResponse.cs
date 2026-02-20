#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertStatusLevelAggregate.Dtos;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Response model for an alert status level
/// </summary>
public sealed class AlertStatusLevelResponse : DataModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public bool SendNotifications { get; set; }

    public bool IsDefault { get; set; }

    public bool IsCanceled { get; set; }
}
