namespace AlertManagement.Core.DTOs.Requests;

/// <summary>
/// Request to update an alert type and the users notified for it.
/// </summary>
public sealed record UpdateAlertTypeRequest(string Name, string Description, string BackgroundColor, string TextColor, IReadOnlyList<string> NotifyUserIds);
