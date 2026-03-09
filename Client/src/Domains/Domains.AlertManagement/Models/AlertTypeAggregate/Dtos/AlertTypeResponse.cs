namespace Domains.AlertManagement.AlertTypeAggregate.Dtos;

/// <summary>
/// Response model for an alert type
/// </summary>
public sealed class AlertTypeResponse : DataModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string BackgroundColor { get; set; } = string.Empty;

    public string TextColor { get; set; } = string.Empty;

    public IReadOnlyList<string> NotifyUserIds { get; set; } = [];
}
