namespace AlertManagement.Core.DTOs.Requests;

public sealed record CreateAlertTypeRequest(string Name, string Description, string BackgroundColor, string TextColor, IReadOnlyList<string> NotifyUserIds);
