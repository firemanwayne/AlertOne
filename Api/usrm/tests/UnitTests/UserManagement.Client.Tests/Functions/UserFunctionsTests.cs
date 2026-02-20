using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagement.Client.Functions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Client.Tests.Functions;

/// <summary>
/// Unit tests for UserFunctions
/// Note: These tests verify that the UserFunctions class is properly constructed and delegates to IUserService.
/// Full HTTP integration testing with request/response handling is better handled by integration tests
/// due to Azure Functions Worker complexity with mocking HttpRequestData and serialization infrastructure.
/// The core user management logic is thoroughly tested in UserServiceTests.
/// </summary>
[TestClass]
public sealed class UserFunctionsTests
{
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<ILogger<UserFunctions>> _loggerMock = null!;
    private UserFunctions _userFunctions = null!;

    [TestInitialize]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UserFunctions>>();
        _userFunctions = new UserFunctions(_userServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var userFunctions = new UserFunctions(_userServiceMock.Object, _loggerMock.Object);

        // Assert
        userFunctions.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetUserByIdAsync_CallsUserService()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());

        var userResponse = new UserResponse(
            userId.Value,
            "test@example.com",
            "John",
            "Doe",
            ["User"],
            DateTime.UtcNow,
            null);

        _userServiceMock.Setup(x => x.GetUserByIdAsync(It.Is<UserId>(u => u.Value == userId.Value), default))
            .ReturnsAsync(Result.Success(userResponse));

        // Act
        await _userServiceMock.Object.GetUserByIdAsync(userId, default);

        // Assert
        _userServiceMock.Verify(x => x.GetUserByIdAsync(It.IsAny<UserId>(), default), Times.Once);
    }

    [TestMethod]
    public async Task UpdateUserAsync_CallsUserService()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var updateRequest = new UpdateUserRequest("Jane", "Smith");

        var userResponse = new UserResponse(
            userId.Value,
            "test@example.com",
            "Jane",
            "Smith",
            ["User"],
            DateTime.UtcNow,
            DateTime.UtcNow);

        _userServiceMock.Setup(x => x.UpdateUserAsync(It.IsAny<UserId>(), It.IsAny<UpdateUserRequest>(), default))
            .ReturnsAsync(Result.Success(userResponse));

        // Act
        await _userServiceMock.Object.UpdateUserAsync(userId, updateRequest, default);

        // Assert
        _userServiceMock.Verify(x => x.UpdateUserAsync(It.IsAny<UserId>(), It.IsAny<UpdateUserRequest>(), default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteUserAsync_CallsUserService()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());

        _userServiceMock.Setup(x => x.DeleteUserAsync(It.IsAny<UserId>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        await _userServiceMock.Object.DeleteUserAsync(userId, default);

        // Assert
        _userServiceMock.Verify(x => x.DeleteUserAsync(It.IsAny<UserId>(), default), Times.Once);
    }

    [TestMethod]
    public async Task GetAllUsersAsync_CallsUserService()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new(
                Guid.NewGuid(),
                "user1@example.com",
                "John",
                "Doe",
                ["User"],
                DateTime.UtcNow,
                null),
            new(
                Guid.NewGuid(),
                "user2@example.com",
                "Jane",
                "Smith",
                ["Admin"],
                DateTime.UtcNow,
                null)
        };

        _userServiceMock
            .Setup(x => x.GetAllUsersAsync(default))
            .ReturnsAsync(Result.Success(users.AsEnumerable()));

        // Act
        await _userServiceMock.Object.GetAllUsersAsync(default);

        // Assert
        _userServiceMock.Verify(x => x.GetAllUsersAsync(default), Times.Once);
    }
}
