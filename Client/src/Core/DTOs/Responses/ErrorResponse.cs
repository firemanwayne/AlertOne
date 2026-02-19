namespace AlertOne.Core.DTOs.Responses;

/// <summary>
/// Response model for error information
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message);
