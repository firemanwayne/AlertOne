namespace AlertManagement.Core.DTOs.Requests;

public sealed record UpdateAlertTypeRequest(string Name, string Description, string Color, IReadOnlyList<string> NotifyUserIds);
