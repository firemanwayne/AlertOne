namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request model for user login
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password,
    DeviceInfo Device);
