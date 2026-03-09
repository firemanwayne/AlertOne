using AlertOne.Api.Core.Abstractions;

namespace AlertManagement.Core.Entities;

public class AlertType : DataModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;

    // EF Core navigation property
    public ICollection<AlertTypeUser> AlertTypeUsers { get; set; } = null!;
}
