namespace AlertManagement.Core.DTOs.Requests;

public sealed record UpdateAlertTypeRequest(string Name, string Description, string BackgroundColor, string TextColor, IReadOnlyList<string> NotifyUserIds);
