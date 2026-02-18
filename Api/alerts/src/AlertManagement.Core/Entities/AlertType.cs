namespace AlertManagement.Core.Entities;

public class AlertType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    // EF Core navigation property
    public ICollection<AlertTypeUser> AlertTypeUsers { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
