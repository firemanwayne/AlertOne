using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;
using UserManagement.Infrastructure.Services;
using UserManagement.Infrastructure.Tests.Helpers;

namespace UserManagement.Infrastructure.Tests.Services;

[TestClass]
public sealed class RoleServiceTests
{
    private Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock = null!;

    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private Mock<ILogger<RoleService>> _loggerMock = null!;

    private RoleService _roleService = null!;

    private ApplicationDbContext _dbContext = null!;

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestInitialize]
    public void Setup()
    {
        _roleManagerMock = MockHelpers.MockRoleManager();
        _userManagerMock = MockHelpers.MockUserManager();
        _loggerMock = new Mock<ILogger<RoleService>>();
        _dbContext = TestDbContextFactory.Create();

        _roleService = new RoleService(
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _dbContext,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task CreateRoleAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateRoleRequest("Manager");

        _roleManagerMock.Setup(x => x.FindByNameAsync(request.Name))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole<Guid>>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleService.CreateRoleAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
        result.Value.UserCount.Should().Be(0);
    }

    [TestMethod]
    public async Task CreateRoleAsync_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateRoleRequest("");

        // Act
        var result = await _roleService.CreateRoleAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NameRequired");
        }
    }

    [TestMethod]
    public async Task CreateRoleAsync_DuplicateName_ReturnsConflictError()
    {
        // Arrange
        var request = new CreateRoleRequest("Admin");
        var existingRole = new IdentityRole<Guid> { Name = "Admin" };

        _roleManagerMock.Setup(x => x.FindByNameAsync(request.Name))
            .ReturnsAsync(existingRole);

        // Act
        var result = await _roleService.CreateRoleAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.AlreadyExists");
        }
    }

    [TestMethod]
    public async Task GetRoleByIdAsync_ValidRole_ReturnsRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Manager"
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync([]);

        // Act
        var result = await _roleService.GetRoleByIdAsync(roleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(roleId);
        result.Value.Name.Should().Be("Manager");
        result.Value.UserCount.Should().Be(0);
    }

    [TestMethod]
    public async Task GetRoleByIdAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.GetRoleByIdAsync(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task GetRoleByNameAsync_ValidRole_ReturnsRole()
    {
        // Arrange
        const string roleName = "Manager";
        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = roleName
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
            .ReturnsAsync([]);

        // Act
        var result = await _roleService.GetRoleByNameAsync(roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(roleName);
    }

    [TestMethod]
    public async Task GetRoleByNameAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        const string roleName = "NonExistent";

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.GetRoleByNameAsync(roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task UpdateRoleAsync_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Manager"
        };
        var request = new UpdateRoleRequest("Senior Manager");

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _roleManagerMock.Setup(x => x.FindByNameAsync(request.Name))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        _roleManagerMock.Setup(x => x.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(request.Name))
            .ReturnsAsync([]);

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(request.Name);
    }

    [TestMethod]
    public async Task UpdateRoleAsync_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest("");

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NameRequired");
        }
    }

    [TestMethod]
    public async Task UpdateRoleAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest("Manager");

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task UpdateRoleAsync_DuplicateName_ReturnsConflictError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Manager"
        };
        var existingRole = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = "Admin"
        };
        var request = new UpdateRoleRequest("Admin");

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _roleManagerMock.Setup(x => x.FindByNameAsync(request.Name))
            .ReturnsAsync(existingRole);

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NameTaken");
        }
    }

    [TestMethod]
    public async Task DeleteRoleAsync_EmptyRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Manager"
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync([]);

        _roleManagerMock.Setup(x => x.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task DeleteRoleAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task DeleteRoleAsync_RoleHasUsers_ReturnsConflictError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Manager"
        };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = Guid.NewGuid(), Email = "user1@example.com" }
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(users);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.HasUsers");
        }
    }

    [TestMethod]
    public async Task AssignRoleToUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com"
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleService.AssignRoleToUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task AssignRoleToUserAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "NonExistent";

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.AssignRoleToUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task AssignRoleToUserAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleService.AssignRoleToUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task AssignRoleToUserAsync_UserAlreadyHasRole_ReturnsConflictError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com"
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.AssignRoleToUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.AlreadyInRole");
        }
    }

    [TestMethod]
    public async Task RemoveRoleFromUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com"
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleService.RemoveRoleFromUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveRoleFromUserAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "NonExistent";

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.RemoveRoleFromUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task RemoveRoleFromUserAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleService.RemoveRoleFromUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotFound");
        }
    }

    [TestMethod]
    public async Task RemoveRoleFromUserAsync_UserDoesNotHaveRole_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com"
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(false);

        // Act
        var result = await _roleService.RemoveRoleFromUserAsync(UserId.From(userId), roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.NotInRole");
        }
    }

    [TestMethod]
    public async Task GetUsersInRoleAsync_ValidRole_ReturnsUsers()
    {
        // Arrange
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
            .ReturnsAsync(users);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([roleName]);

        // Act
        var result = await _roleService.GetUsersInRoleAsync(roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Email.Should().Be("user1@example.com");
    }

    [TestMethod]
    public async Task GetUsersInRoleAsync_RoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        const string roleName = "NonExistent";

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole<Guid>?)null);

        // Act
        var result = await _roleService.GetUsersInRoleAsync(roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Role.NotFound");
        }
    }

    [TestMethod]
    public async Task GetUsersInRoleAsync_EmptyRole_ReturnsEmptyList()
    {
        // Arrange
        const string roleName = "Manager";
        var role = new IdentityRole<Guid> { Name = roleName };

        _roleManagerMock.Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
            .ReturnsAsync([]);

        // Act
        var result = await _roleService.GetUsersInRoleAsync(roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetAllRolesAsync_NoRoles_ReturnsEmptyList()
    {
        // Act
        var result = await _roleService.GetAllRolesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetAllRolesAsync_WithRoles_ReturnsEveryRoleWithUserCount()
    {
        // Arrange
        var admin = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" };
        var user = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" };

        _dbContext.Roles.AddRange(admin, user);

        await _dbContext.SaveChangesAsync();

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync("Admin"))
            .ReturnsAsync([new ApplicationUser(), new ApplicationUser()]);
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync("User"))
            .ReturnsAsync([]);

        // Act
        var result = await _roleService.GetAllRolesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Single(r => r.Name == "Admin").UserCount.Should().Be(2);
        result.Value.Single(r => r.Name == "User").UserCount.Should().Be(0);
    }
}
