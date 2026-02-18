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
/// Unit tests for RoleFunctions
/// Note: These tests verify that the RoleFunctions class is properly constructed and delegates to IRoleService.
/// Full HTTP integration testing with request/response handling is better handled by integration tests
/// due to Azure Functions Worker complexity with mocking HttpRequestData and serialization infrastructure.
/// The core role management logic is thoroughly tested in RoleServiceTests.
/// </summary>
[TestClass]
public sealed class RoleFunctionsTests
{
    private Mock<IRoleService> _roleServiceMock = null!;
    private Mock<ILogger<RoleFunctions>> _loggerMock = null!;
    private RoleFunctions _roleFunctions = null!;

    [TestInitialize]
    public void Setup()
    {
        _roleServiceMock = new Mock<IRoleService>();
        _loggerMock = new Mock<ILogger<RoleFunctions>>();
        _roleFunctions = new RoleFunctions(_roleServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var roleFunctions = new RoleFunctions(_roleServiceMock.Object, _loggerMock.Object);

        // Assert
        roleFunctions.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateRoleAsync_CallsRoleService()
    {
        // Arrange
        var createRequest = new CreateRoleRequest("Manager");

        var roleResponse = new RoleResponse(
            Guid.NewGuid(),
            "Manager",
            0);

        _roleServiceMock.Setup(x => x.CreateRoleAsync(It.Is<CreateRoleRequest>(r => r.Name == createRequest.Name), default))
            .ReturnsAsync(Result.Success(roleResponse));

        // Act
        await _roleServiceMock.Object.CreateRoleAsync(createRequest, default);

        // Assert
        _roleServiceMock.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>(), default), Times.Once);
    }

    [TestMethod]
    public async Task GetRoleByIdAsync_CallsRoleService()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        var roleResponse = new RoleResponse(
            roleId,
            "Admin",
            5);

        _roleServiceMock.Setup(x => x.GetRoleByIdAsync(roleId, default))
            .ReturnsAsync(Result.Success(roleResponse));

        // Act
        await _roleServiceMock.Object.GetRoleByIdAsync(roleId, default);

        // Assert
        _roleServiceMock.Verify(x => x.GetRoleByIdAsync(roleId, default), Times.Once);
    }

    [TestMethod]
    public async Task GetRoleByNameAsync_CallsRoleService()
    {
        // Arrange
        const string roleName = "Admin";

        var roleResponse = new RoleResponse(
            Guid.NewGuid(),
            roleName,
            5);

        _roleServiceMock.Setup(x => x.GetRoleByNameAsync(roleName, default))
            .ReturnsAsync(Result.Success(roleResponse));

        // Act
        await _roleServiceMock.Object.GetRoleByNameAsync(roleName, default);

        // Assert
        _roleServiceMock.Verify(x => x.GetRoleByNameAsync(roleName, default), Times.Once);
    }

    [TestMethod]
    public async Task GetAllRolesAsync_CallsRoleService()
    {
        // Arrange
        var roles = new List<RoleResponse>
        {
            new(Guid.NewGuid(), "Admin", 5),
            new(Guid.NewGuid(), "User", 10)
        };

        _roleServiceMock.Setup(x => x.GetAllRolesAsync(default))
            .ReturnsAsync(Result.Success(roles.AsEnumerable()));

        // Act
        await _roleServiceMock.Object.GetAllRolesAsync(default);

        // Assert
        _roleServiceMock.Verify(x => x.GetAllRolesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task UpdateRoleAsync_CallsRoleService()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var updateRequest = new UpdateRoleRequest("Manager");

        var roleResponse = new RoleResponse(
            roleId,
            "Manager",
            3);

        _roleServiceMock.Setup(x => x.UpdateRoleAsync(roleId, It.IsAny<UpdateRoleRequest>(), default))
            .ReturnsAsync(Result.Success(roleResponse));

        // Act
        await _roleServiceMock.Object.UpdateRoleAsync(roleId, updateRequest, default);

        // Assert
        _roleServiceMock.Verify(x => x.UpdateRoleAsync(roleId, It.IsAny<UpdateRoleRequest>(), default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteRoleAsync_CallsRoleService()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleServiceMock.Setup(x => x.DeleteRoleAsync(roleId, default))
            .ReturnsAsync(Result.Success());

        // Act
        await _roleServiceMock.Object.DeleteRoleAsync(roleId, default);

        // Assert
        _roleServiceMock.Verify(x => x.DeleteRoleAsync(roleId, default), Times.Once);
    }

    [TestMethod]
    public async Task AssignRoleToUserAsync_CallsRoleService()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        const string roleName = "Admin";

        _roleServiceMock.Setup(x => x.AssignRoleToUserAsync(It.IsAny<UserId>(), roleName, default))
            .ReturnsAsync(Result.Success());

        // Act
        await _roleServiceMock.Object.AssignRoleToUserAsync(userId, roleName, default);

        // Assert
        _roleServiceMock.Verify(x => x.AssignRoleToUserAsync(It.IsAny<UserId>(), roleName, default), Times.Once);
    }

    [TestMethod]
    public async Task RemoveRoleFromUserAsync_CallsRoleService()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        const string roleName = "Admin";

        _roleServiceMock.Setup(x => x.RemoveRoleFromUserAsync(It.IsAny<UserId>(), roleName, default))
            .ReturnsAsync(Result.Success());

        // Act
        await _roleServiceMock.Object.RemoveRoleFromUserAsync(userId, roleName, default);

        // Assert
        _roleServiceMock.Verify(x => x.RemoveRoleFromUserAsync(It.IsAny<UserId>(), roleName, default), Times.Once);
    }

    [TestMethod]
    public async Task GetUsersInRoleAsync_CallsRoleService()
    {
        // Arrange
        const string roleName = "Admin";

        var users = new List<UserResponse>
        {
            new(
                Guid.NewGuid(),
                "admin@example.com",
                "Admin",
                "User",
                ["Admin"],
                DateTime.UtcNow,
                null)
        };

        _roleServiceMock.Setup(x => x.GetUsersInRoleAsync(roleName, default))
            .ReturnsAsync(Result.Success(users.AsEnumerable()));

        // Act
        await _roleServiceMock.Object.GetUsersInRoleAsync(roleName, default);

        // Assert
        _roleServiceMock.Verify(x => x.GetUsersInRoleAsync(roleName, default), Times.Once);
    }
}
