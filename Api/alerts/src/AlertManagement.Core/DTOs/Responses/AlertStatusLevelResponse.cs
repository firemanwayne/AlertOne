namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertStatusLevelResponse(
    string Id,
    string Name,
    string Description,
    string Color,
    bool SendNotifications,
    DateTime CreatedAt,
    DateTime LastModifiedAt);
