using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Response when validating a refresh token
/// </summary>
public sealed record ValidateRefreshTokenResponse(
    UserId UserId,
    Guid DeviceId);
