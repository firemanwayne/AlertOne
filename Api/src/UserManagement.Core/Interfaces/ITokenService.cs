using System.Security.Claims;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, string firstName, string lastName, IList<string> roles);

    /// <summary>
    /// Generates a secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an expired access token and extracts claims
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
