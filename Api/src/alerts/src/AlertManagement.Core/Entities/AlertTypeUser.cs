using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

public class AlertTypeUser : DataModelBase
{
    public string AlertTypeId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    // EF Core navigation property
    public AlertType AlertType { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
