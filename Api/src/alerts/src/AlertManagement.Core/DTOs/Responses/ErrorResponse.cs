namespace AlertManagement.Core.DTOs.Responses;

/// <summary>
/// Error body returned by the API for failed requests.
/// </summary>
public sealed record ErrorResponse(string Code, string Message);
