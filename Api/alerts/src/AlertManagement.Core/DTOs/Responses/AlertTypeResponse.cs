namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertTypeResponse(
    Guid Id,
    string Name,
    string Description,
    string Color,
    IReadOnlyList<Guid> NotifyUserIds,
    DateTime CreatedAt,
    DateTime LastModifiedAt);
