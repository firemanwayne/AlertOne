namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request model for assigning a role to a user
/// </summary>
public sealed record AssignRoleRequest(Guid UserId, string RoleName);
