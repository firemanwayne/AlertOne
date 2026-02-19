namespace AlertManagement.Core.Entities;

public class Alert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AlertTypeId { get; set; } = string.Empty;
    public string AlertStatusLevelId { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RescindedAt { get; set; }
    public string? RescindedBy { get; set; }

    // EF Core navigation properties
    public AlertType AlertType { get; set; } = null!; // EF Core navigation property - always loaded via Include
    public AlertStatusLevel AlertStatusLevel { get; set; } = null!; // EF Core navigation property - always loaded via Include
}
