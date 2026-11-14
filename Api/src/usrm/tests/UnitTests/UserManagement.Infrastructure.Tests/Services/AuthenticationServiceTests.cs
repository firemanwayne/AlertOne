using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Enums;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;
using UserManagement.Infrastructure.Services;
using UserManagement.Infrastructure.Tests.Helpers;

namespace UserManagement.Infrastructure.Tests.Services;

[TestClass]
public sealed class AuthenticationServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private Mock<SignInManager<ApplicationUser>> _signInManagerMock = null!;

    private Mock<ITokenService> _tokenServiceMock = null!;

    private Mock<IDeviceService> _deviceServiceMock = null!;

    private Mock<IUserSettingsService> _userSettingsServiceMock = null!;

    private Mock<ILogger<AuthenticationService>> _loggerMock = null!;

    private AuthenticationService _authenticationService = null!;

    private ApplicationDbContext _dbContext = null!;

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestInitialize]
    public void Setup()
    {
        _userManagerMock = MockHelpers.MockUserManager();
        _signInManagerMock = MockHelpers.MockSignInManager(_userManagerMock);
        _tokenServiceMock = new Mock<ITokenService>();
        _deviceServiceMock = new Mock<IDeviceService>();
        _userSettingsServiceMock = new Mock<IUserSettingsService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _dbContext = TestDbContextFactory.Create();

        _authenticationService = new AuthenticationService(
            _tokenServiceMock.Object,
            _deviceServiceMock.Object,
            _userSettingsServiceMock.Object,
            _loggerMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _dbContext);
    }

    [TestMethod]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Password123!",
            "John",
            "Doe",
            deviceInfo);

        var deviceSession = CreateTestDeviceSession();

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["User"]);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("access_token");

        _deviceServiceMock.Setup(x => x.RegisterOrUpdateDeviceAsync(It.IsAny<UserId>(), deviceInfo, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new Core.DTOs.Responses.RegisterDeviceResponse("refresh_token", deviceSession)));

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.User.Email.Should().Be(request.Email.ToLowerInvariant());
        result.Value.CurrentDevice.Should().NotBeNull();
    }

    [TestMethod]
    public async Task RegisterAsync_DuplicateEmail_ReturnsConflictError()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Password123!",
            "John",
            "Doe",
            deviceInfo);

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser { Email = request.Email });

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.AlreadyExists");
        }
    }

    [TestMethod]
    public async Task RegisterAsync_PasswordMismatch_ReturnsValidationError()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "DifferentPassword123!",
            "John",
            "Doe",
            deviceInfo);

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();

        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Password.Mismatch");
        }
    }

    [TestMethod]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new LoginRequest("test@example.com", "Password123!", deviceInfo);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        var deviceSession = CreateTestDeviceSession();

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, It.IsAny<IList<string>>()))
            .Returns("access_token");

        _deviceServiceMock.Setup(x => x.RegisterOrUpdateDeviceAsync(It.IsAny<UserId>(), deviceInfo, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new Core.DTOs.Responses.RegisterDeviceResponse("refresh_token", deviceSession)));

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.CurrentDevice.Should().NotBeNull();
    }

    [TestMethod]
    public async Task LoginAsync_InvalidEmail_ReturnsUnauthorizedError()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new LoginRequest("nonexistent@example.com", "Password123!", deviceInfo);

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Auth.InvalidCredentials");
        }
    }

    [TestMethod]
    public async Task LoginAsync_InvalidPassword_ReturnsUnauthorizedError()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new LoginRequest("test@example.com", "WrongPassword123!", deviceInfo);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Auth.InvalidCredentials");
        }
    }

    [TestMethod]
    public async Task LoginAsync_AccountLockedOut_ReturnsForbiddenError()
    {
        // Arrange
        var deviceInfo = CreateTestDeviceInfo();
        var request = new LoginRequest("test@example.com", "Password123!", deviceInfo);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();

        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Auth.LockedOut");
        }
    }

    [TestMethod]
    public async Task RefreshTokenAsync_ValidTokens_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var request = new RefreshTokenRequest("valid_refresh_token");

        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var deviceSession = CreateTestDeviceSession();

        _deviceServiceMock.Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new Core.DTOs.Responses.ValidateRefreshTokenResponse(UserId.From(userId), deviceId)));

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, It.IsAny<IList<string>>()))
            .Returns("new_access_token");

        _deviceServiceMock.Setup(x => x.UpdateLastActivityAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _deviceServiceMock.Setup(x => x.GetDeviceByIdAsync(UserId.From(userId), deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(deviceSession));

        // Act
        var result = await _authenticationService.RefreshTokenAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be(request.RefreshToken);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsUnauthorizedError()
    {
        // Arrange
        var request = new RefreshTokenRequest("invalid_refresh_token");

        _deviceServiceMock.Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Core.DTOs.Responses.ValidateRefreshTokenResponse>(Error.Unauthorized("RefreshToken.Invalid", "Invalid refresh token")));

        // Act
        var result = await _authenticationService.RefreshTokenAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("RefreshToken.Invalid");
        }
    }

    [TestMethod]
    public async Task RevokeTokenAsync_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _deviceServiceMock.Setup(x => x.RevokeAllDevicesAsync(UserId.From(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _authenticationService.RevokeTokenAsync(UserId.From(userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task RevokeTokenAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authenticationService.RevokeTokenAsync(UserId.From(userId));

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task HasUsersAsync_NoUsers_ReturnsFalse()
    {
        // Act
        var result = await _authenticationService.HasUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [TestMethod]
    public async Task HasUsersAsync_WithAnUser_ReturnsTrue()
    {
        // Arrange
        _dbContext.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), Email = "first@example.com", FirstName = "First", LastName = "User" });

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authenticationService.HasUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    private static DeviceInfo CreateTestDeviceInfo()
    {
        return new DeviceInfo(
            "test-device-id",
            "Test Device",
            DeviceType.Web,
            "Windows",
            "1.0.0",
            "Mozilla/5.0",
            "test-push-token");
    }

    private static DeviceSessionResponse CreateTestDeviceSession()
    {
        return new DeviceSessionResponse(
            Guid.NewGuid(),
            "Test Device",
            DeviceType.Web,
            "Windows",
            "1.0.0",
            false,
            true,
            "127.0.0.1",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            "test-push-token");
    }
}
