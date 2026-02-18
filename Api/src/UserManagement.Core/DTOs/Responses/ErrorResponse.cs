namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Response model for error information
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? ValidationErrors = null);
