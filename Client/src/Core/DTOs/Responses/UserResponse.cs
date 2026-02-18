namespace UserManagementApps.Core.DTOs.Responses;

/// <summary>
/// Response model for user information
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);
