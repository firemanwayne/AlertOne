namespace AlertManagement.Core.Entities;

public class AlertStatusLevel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool SendNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
