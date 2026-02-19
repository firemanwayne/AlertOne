namespace Domains.AlertManagement.AlertTypeAggregate.Dtos;

/// <summary>
/// Response model for an alert type
/// </summary>
public sealed class AlertTypeResponse : DataModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public IReadOnlyList<string> NotifyUserIds { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }
}
