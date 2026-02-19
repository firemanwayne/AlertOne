namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request model for updating user information
/// </summary>
public sealed record UpdateUserRequest(
    string? FirstName,
    string? LastName);
