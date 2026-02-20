namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request to create a new user (admin operation)
/// </summary>
public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string[]? Roles = null);
