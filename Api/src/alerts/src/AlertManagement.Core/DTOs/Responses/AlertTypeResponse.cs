namespace AlertManagement.Core.DTOs.Responses;

/// <summary>
/// An alert type and the users notified for it.
/// </summary>
public sealed record AlertTypeResponse(
    string Id,
    string Name,
    string Description,
    string BackgroundColor,
    string TextColor,
    IReadOnlyList<string> NotifyUserIds,
    DateTime CreatedDate,
    DateTime UpdatedDate);
