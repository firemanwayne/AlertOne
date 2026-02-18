namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request model for user registration
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    DeviceInfo Device);
