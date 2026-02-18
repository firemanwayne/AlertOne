namespace AlertManagement.Core.DTOs.Responses;

public sealed record AlertHistoryResponse(
    Guid Id,
    Guid AlertId,
    string EventType,
    string ChangedBy,
    DateTime ChangedAt,
    string? Notes);
