namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertStatusLevelResponse(
    string Id,
    string Name,
    string Description,
    string BackgroundColor,
    string TextColor,
    bool SendNotifications,
    bool IsDefault,
    bool IsCanceled,
    DateTime CreatedDate,
    DateTime UpdatedDate);
