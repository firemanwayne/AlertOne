namespace Domains.AlertManagement.AlertAggregate.Dtos;

public sealed class AlertHistoryResponse : DataModelBase
{
    public string AlertId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
