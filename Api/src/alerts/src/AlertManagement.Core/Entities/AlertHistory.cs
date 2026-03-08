using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

public class AlertHistory : DataModelBase
{
    public string AlertId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Alert Alert { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
