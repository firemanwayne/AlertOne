using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Infrastructure.Configurations;
using UserManagement.Infrastructure.Services;

namespace UserManagement.Infrastructure.Tests.Services;

[TestClass]
public sealed class TokenServiceTests
{
    private Mock<IOptions<JwtSettings>> _jwtSettingsMock = null!;
    private TokenService _tokenService = null!;
    private JwtSettings _jwtSettings = null!;

    [TestInitialize]
    public void Setup()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-very-secure-secret-key-for-testing-purposes-minimum-256-bits",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };

        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _jwtSettingsMock.Setup(x => x.Value).Returns(_jwtSettings);

        _tokenService = new TokenService(_jwtSettingsMock.Object);
    }

    [TestMethod]
    public void GenerateAccessToken_ValidParameters_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User" };

        // Act
        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void GenerateAccessToken_ValidToken_ContainsClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User", "Admin" };

        // Act
        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == firstName);
        jwtToken.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == lastName);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [TestMethod]
    public void GenerateAccessToken_ValidToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User" };

        // Act
        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
    }

    [TestMethod]
    public void GenerateAccessToken_ValidToken_HasCorrectExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User" };

        // Act
        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [TestMethod]
    public void GenerateAccessToken_EmptyRoles_CreatesTokenWithoutRoleClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string>();

        // Act
        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }

    [TestMethod]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(40); // Base64 encoded 32 bytes should be ~44 characters
    }

    [TestMethod]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [TestMethod]
    public void GetPrincipalFromExpiredToken_ValidExpiredToken_ReturnsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User" };

        // Generate a token with very short expiry
        var shortExpirySettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            ExpiryMinutes = -1 // Already expired
        };

        var shortExpirySettingsMock = new Mock<IOptions<JwtSettings>>();
        shortExpirySettingsMock.Setup(x => x.Value).Returns(shortExpirySettings);

        var shortExpiryTokenService = new TokenService(shortExpirySettingsMock.Object);
        var expiredToken = shortExpiryTokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(expiredToken);

        // Assert
        principal.Should().NotBeNull();
        if (principal is not null)
        {
            // After validation, Sub becomes NameIdentifier
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
            // After validation, Email becomes EmailAddress
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        }
    }

    [TestMethod]
    public void GetPrincipalFromExpiredToken_ValidNonExpiredToken_ReturnsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var roles = new List<string> { "User" };

        var token = _tokenService.GenerateAccessToken(userId, email, firstName, lastName, roles);

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        if (principal is not null)
        {
            // After validation, Sub becomes NameIdentifier
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        }
    }

    [TestMethod]
    public void GetPrincipalFromExpiredToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        const string invalidToken = "invalid.token.value";

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [TestMethod]
    public void GetPrincipalFromExpiredToken_EmptyToken_ReturnsNull()
    {
        // Arrange
        const string emptyToken = "";

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(emptyToken);

        // Assert
        principal.Should().BeNull();
    }

    [TestMethod]
    public void GetPrincipalFromExpiredToken_TokenWithWrongSigningAlgorithm_ReturnsNull()
    {
        // Arrange - Create a token with a different secret
        var differentSettings = new JwtSettings
        {
            Secret = "different-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            ExpiryMinutes = 60
        };

        var differentSettingsMock = new Mock<IOptions<JwtSettings>>();
        differentSettingsMock.Setup(x => x.Value).Returns(differentSettings);

        var differentTokenService = new TokenService(differentSettingsMock.Object);
        var tokenWithDifferentSecret = differentTokenService.GenerateAccessToken(
            Guid.NewGuid(), "test@example.com", "John", "Doe", ["User"]);

        // Act - Try to validate with original secret
        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenWithDifferentSecret);

        // Assert
        principal.Should().BeNull();
    }
}
