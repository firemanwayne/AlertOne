namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to create an alert type and the users notified for it.
/// </summary>
public sealed record CreateAlertTypeRequest(string Name, string Description, string BackgroundColor, string TextColor, IReadOnlyList<string> NotifyUserIds);
