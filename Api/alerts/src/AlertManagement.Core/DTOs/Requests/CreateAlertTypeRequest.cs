namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertTypeRequest(string Name, string Description, string Color, IReadOnlyList<Guid> NotifyUserIds);
