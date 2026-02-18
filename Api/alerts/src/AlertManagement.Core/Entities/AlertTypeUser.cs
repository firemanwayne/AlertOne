namespace AlertManagement.Core.Entities;

public class AlertTypeUser
{
    public Guid AlertTypeId { get; set; }
    public Guid UserId { get; set; }

    // EF Core navigation property
    public AlertType AlertType { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
