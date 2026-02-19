namespace AlertManagement.Core.Entities;

public class AlertHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AlertId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
    public Alert Alert { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
