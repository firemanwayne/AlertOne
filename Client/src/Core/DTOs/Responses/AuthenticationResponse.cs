namespace AlertOne.Core.DTOs.Responses;

/// <summary>
/// Response model for authentication operations
/// </summary>
public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User,
    IEnumerable<UserClaim> Claims,
    DeviceSessionResponse CurrentDevice);
