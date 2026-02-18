namespace UserManagementApps.Core.DTOs.Requests;

/// <summary>
/// Request model for creating a new role
/// </summary>
public sealed record CreateRoleRequest(
    string RoleName);
