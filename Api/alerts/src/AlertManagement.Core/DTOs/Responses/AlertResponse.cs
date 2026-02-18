namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertResponse(
    Guid Id,
    AlertTypeResponse Type,
    AlertStatusLevelResponse StatusLevel,
    string CreatedBy,
    string? LastModifiedBy,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    bool IsActive,
    DateTime? RescindedAt,
    string? RescindedBy);
