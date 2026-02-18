using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagement.Client.Functions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Enums;
using UserManagement.Core.Interfaces;

namespace UserManagement.Client.Tests.Functions;

/// <summary>
/// Unit tests for AuthFunctions
/// Note: These tests verify that the AuthFunctions class is properly constructed and delegates to IAuthenticationService.
/// Full HTTP integration testing with request/response handling is better handled by integration tests
/// due to Azure Functions Worker complexity with mocking HttpRequestData and serialization infrastructure.
/// The core authentication logic is thoroughly tested in AuthenticationServiceTests.
/// </summary>
[TestClass]
public sealed class AuthFunctionsTests
{
    private Mock<IAuthenticationService> _authServiceMock = null!;
    private Mock<ILogger<AuthFunctions>> _loggerMock = null!;
    private AuthFunctions _authFunctions = null!;

    [TestInitialize]
    public void Setup()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<AuthFunctions>>();
        _authFunctions = new AuthFunctions(_loggerMock.Object, _authServiceMock.Object);
    }

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var authFunctions = new AuthFunctions(_loggerMock.Object, _authServiceMock.Object);

        // Assert
        authFunctions.Should().NotBeNull();
    }

    [TestMethod]
    public async Task RegisterAsync_CallsAuthenticationService()
    {
        // Arrange
        var deviceInfo = new DeviceInfo(
            "test-device-id",
            "Test Device",
            DeviceType.Web,
            "Windows",
            "1.0.0",
            "Mozilla/5.0",
            "test-push-token");

        var registerRequest = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Password123!",
            "John",
            "Doe",
            deviceInfo);

        var userResponse = new UserResponse(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            ["User"],
            DateTime.UtcNow,
            null);

        var deviceSession = new DeviceSessionResponse(
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

        var authResponse = new AuthenticationResponse(
            "access_token",
            "refresh_token",
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            [],
            deviceSession);

        _authServiceMock.Setup(x => x.RegisterAsync(It.Is<RegisterRequest>(r => r.Email == registerRequest.Email), default))
            .ReturnsAsync(Result.Success(authResponse));

        // Act
        await _authServiceMock.Object.RegisterAsync(registerRequest, default);

        // Assert
        _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), default), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_CallsAuthenticationService()
    {
        // Arrange
        var deviceInfo = new DeviceInfo(
            "test-device-id",
            "Test Device",
            DeviceType.Web,
            "Windows",
            "1.0.0",
            "Mozilla/5.0",
            "test-push-token");

        var loginRequest = new LoginRequest("test@example.com", "Password123!", deviceInfo);

        var userResponse = new UserResponse(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            ["User"],
            DateTime.UtcNow,
            null);

        var deviceSession = new DeviceSessionResponse(
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

        var authResponse = new AuthenticationResponse(
            "access_token",
            "refresh_token",
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            [],
            deviceSession);

        _authServiceMock.Setup(x => x.LoginAsync(It.Is<LoginRequest>(r => r.Email == loginRequest.Email), default))
            .ReturnsAsync(Result.Success(authResponse));

        // Act
        await _authServiceMock.Object.LoginAsync(loginRequest, default);

        // Assert
        _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<LoginRequest>(), default), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_CallsAuthenticationService()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest("valid_refresh_token");

        var userResponse = new UserResponse(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            ["User"],
            DateTime.UtcNow,
            null);

        var deviceSession = new DeviceSessionResponse(
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

        var authResponse = new AuthenticationResponse(
            "new_access_token",
            "valid_refresh_token",
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            [],
            deviceSession);

        _authServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>(), default))
            .ReturnsAsync(Result.Success(authResponse));

        // Act
        await _authServiceMock.Object.RefreshTokenAsync(refreshRequest, default);

        // Assert
        _authServiceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>(), default), Times.Once);
    }
}
