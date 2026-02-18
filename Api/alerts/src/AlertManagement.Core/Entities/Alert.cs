namespace AlertManagement.Core.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid AlertTypeId { get; set; }
    public Guid AlertStatusLevelId { get; set; }
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
