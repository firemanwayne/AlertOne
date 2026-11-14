namespace AlertManagement.Core.DTOs.Responses;

/// <summary>
/// One entry in the history of an alert.
/// </summary>
public sealed record AlertHistoryResponse(
    string Id,
    string AlertId,
    string EventType,
    string ChangedBy,
    DateTime ChangedAt,
    string? Notes);
