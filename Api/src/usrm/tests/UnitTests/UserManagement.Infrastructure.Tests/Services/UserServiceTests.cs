using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;
using UserManagement.Infrastructure.Services;
using UserManagement.Infrastructure.Tests.Helpers;

namespace UserManagement.Infrastructure.Tests.Services;

[TestClass]
public sealed class UserServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private Mock<ILogger<UserService>> _loggerMock = null!;

    private Mock<IUserSettingsService> _userSettingsServiceMock = null!;

    private UserService _userService = null!;

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
        _loggerMock = new Mock<ILogger<UserService>>();
        _userSettingsServiceMock = new Mock<IUserSettingsService>();
        _dbContext = TestDbContextFactory.Create();

        _userService = new UserService(_loggerMock.Object, _userManagerMock.Object, _dbContext, _userSettingsServiceMock.Object);
    }

    [TestMethod]
    public async Task GetUserByIdAsync_ValidUserId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = null
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.GetUserByIdAsync(UserId.From(userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be(user.Email);
        result.Value.FirstName.Should().Be(user.FirstName);
        result.Value.LastName.Should().Be(user.LastName);
        result.Value.Roles.Should().Contain("User");
    }

    [TestMethod]
    public async Task GetUserByIdAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(UserId.From(userId));

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task GetUserByEmailAsync_ValidEmail_ReturnsUser()
    {
        // Arrange
        const string email = "test@example.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };

        var emailResult = Email.Create(email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.GetUserByEmailAsync(emailResult.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
    }

    [TestMethod]
    public async Task GetUserByEmailAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        var emailResult = Email.Create(email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.GetUserByEmailAsync(emailResult.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task UpdateUserAsync_ValidUpdate_ReturnsUpdatedUser()
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

        var request = new UpdateUserRequest("Jane", "Smith");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.UpdateUserAsync(UserId.From(userId), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.LastModifiedAt.Should().NotBeNull();
    }

    [TestMethod]
    public async Task UpdateUserAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Jane", "Smith");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.UpdateUserAsync(UserId.From(userId), request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task UpdateUserAsync_NullFirstName_OnlyUpdatesLastName()
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

        var request = new UpdateUserRequest(null, "Smith");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.UpdateUserAsync(UserId.From(userId), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("John"); // Unchanged
        user.LastName.Should().Be("Smith");
    }

    [TestMethod]
    public async Task UpdateUserAsync_NullLastName_OnlyUpdatesFirstName()
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

        var request = new UpdateUserRequest("Jane", null);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.UpdateUserAsync(UserId.From(userId), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Doe"); // Unchanged
    }

    [TestMethod]
    public async Task UpdateUserAsync_BothNull_NoChanges()
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

        var request = new UpdateUserRequest(null, null);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.UpdateUserAsync(UserId.From(userId), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("John"); // Unchanged
        user.LastName.Should().Be("Doe"); // Unchanged
    }

    [TestMethod]
    public async Task DeleteUserAsync_ValidUser_ReturnsSuccess()
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

        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.DeleteUserAsync(UserId.From(userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task DeleteUserAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.DeleteUserAsync(UserId.From(userId));

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task DeleteUserAsync_DeleteFails_ReturnsInternalError()
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

        var errors = new[]
        {
            new IdentityError { Code = "DeleteFailed", Description = "Failed to delete user" }
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _userService.DeleteUserAsync(UserId.From(userId));

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.DeleteFailed");
        }
    }

    [TestMethod]
    public async Task GetAllUsersAsync_NoUsers_ReturnsEmptyList()
    {
        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetAllUsersAsync_WithUsers_ReturnsEveryUserWithRoles()
    {
        // Arrange
        var admin = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", FirstName = "Ada", LastName = "Admin" };
        var member = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", FirstName = "Max", LastName = "Member" };

        _dbContext.Users.AddRange(admin, member);

        await _dbContext.SaveChangesAsync();

        _userManagerMock.Setup(x => x.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == admin.Id)))
            .ReturnsAsync(["Admin"]);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == member.Id)))
            .ReturnsAsync(["User"]);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Single(u => u.Id == admin.Id).Roles.Should().BeEquivalentTo(["Admin"]);
        result.Value.Single(u => u.Id == member.Id).Email.Should().Be("member@example.com");
    }
    // These scenarios are better covered by integration tests
}
