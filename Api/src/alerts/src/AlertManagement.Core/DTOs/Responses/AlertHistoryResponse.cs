namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertHistoryResponse(
    string Id,
    string AlertId,
    string EventType,
    string ChangedBy,
    DateTime ChangedAt,
    string? Notes);
