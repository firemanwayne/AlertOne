namespace AlertManagement.Core.DTOs.Responses;

/// <summary>
/// An alert together with its type and status level.
/// </summary>
public sealed record AlertResponse(
    string Id,
    AlertTypeResponse Type,
    AlertStatusLevelResponse StatusLevel,
    string CreatedBy,
    string? UpdatedBy,
    DateTime CreatedDate,
    DateTime UpdatedDate,
    bool IsActive,
    DateTime? RescindedAt,
    string? RescindedBy);
