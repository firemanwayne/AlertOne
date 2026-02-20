namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Response model for role information
/// </summary>
public sealed record RoleResponse(
    Guid Id,
    string Name,
    int UserCount);
