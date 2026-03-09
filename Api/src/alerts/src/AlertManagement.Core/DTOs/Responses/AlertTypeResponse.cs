namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertTypeResponse(
    string Id,
    string Name,
    string Description,
    string BackgroundColor,
    string TextColor,
    IReadOnlyList<string> NotifyUserIds,
    DateTime CreatedDate,
    DateTime UpdatedDate);
