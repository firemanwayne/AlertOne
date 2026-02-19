namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertTypeResponse(
    string Id,
    string Name,
    string Description,
    string Color,
    IReadOnlyList<string> NotifyUserIds,
    DateTime CreatedAt,
    DateTime LastModifiedAt);
