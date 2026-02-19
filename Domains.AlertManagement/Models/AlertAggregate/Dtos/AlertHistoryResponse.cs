namespace Domains.AlertManagement.AlertAggregate.Dtos;

public sealed class AlertHistoryResponse : DataModelBase
{
    public string AlertId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
}
