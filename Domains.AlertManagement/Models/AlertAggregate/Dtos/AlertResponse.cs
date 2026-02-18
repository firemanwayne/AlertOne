using Domains.AlertManagement.AlertStatusLevelAggregate.Dtos;
using Domains.AlertManagement.AlertTypeAggregate.Dtos;
using UserManagementApps.Core.Abstractions;

namespace Domains.AlertManagement.AlertAggregate.Dtos;

/// <summary>
/// Response model for an alert
/// </summary>
public sealed class AlertResponse : DataModelBase
{
    public AlertTypeResponse Type { get; set; } = new();

    public AlertStatusLevelResponse StatusLevel { get; set; } = new();

    public string CreatedBy { get; set; } = string.Empty;

    public string? LastModifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? RescindedAt { get; set; }

    public string? RescindedBy { get; set; }
}
