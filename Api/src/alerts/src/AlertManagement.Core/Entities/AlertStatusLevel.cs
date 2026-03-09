using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

public class AlertStatusLevel : DataModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
    public bool SendNotifications { get; set; }
    public bool IsDefault { get; set; }
    public bool IsCanceled { get; set; }
}
