namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request model for refreshing access token
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);
