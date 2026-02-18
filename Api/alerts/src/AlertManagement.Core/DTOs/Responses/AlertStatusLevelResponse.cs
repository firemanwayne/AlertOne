namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertStatusLevelResponse(
    Guid Id,
    string Name,
    string Description,
    string Color,
    bool SendNotifications,
    DateTime CreatedAt,
    DateTime LastModifiedAt);
