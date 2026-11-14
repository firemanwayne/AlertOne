using FluentAssertions;

using Microsoft.EntityFrameworkCore;
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
public sealed class UserSettingsServiceTests
{
    private ApplicationDbContext _dbContext = null!;

    private UserSettingsService _userSettingsService = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbContext = TestDbContextFactory.Create();

        _userSettingsService = new UserSettingsService(new Mock<ILogger<UserSettingsService>>().Object, _dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetByUserIdAsync_SettingsExist_ReturnsThem()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _dbContext.UserSettings.Add(new UserSettings { Id = Guid.NewGuid().ToString(), UserId = userId, ReceiveEmailNotifications = false, ReceivePushNotifications = true, PreferredTheme = "dark" });

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.GetByUserIdAsync(UserId.From(userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReceiveEmailNotifications.Should().BeFalse();
        result.Value.PreferredTheme.Should().Be("dark");
    }

    [TestMethod]
    public async Task GetByUserIdAsync_UserWithoutSettings_CreatesDefaults()
    {
        // Arrange
        var userId = AddUser();

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.GetByUserIdAsync(UserId.From(userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.ReceiveEmailNotifications.Should().BeTrue();
        result.Value.ReceivePushNotifications.Should().BeTrue();
        (await _dbContext.UserSettings.CountAsync()).Should().Be(1);
    }

    [TestMethod]
    public async Task GetByUserIdAsync_UnknownUser_ReturnsNotFoundAndCreatesNothing()
    {
        // Act
        var result = await _userSettingsService.GetByUserIdAsync(UserId.From(Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("UserSettings.NotFound");
        (await _dbContext.UserSettings.CountAsync()).Should().Be(0);
    }

    [TestMethod]
    public async Task UpdateAsync_UserWithoutSettings_CreatesDefaultsThenAppliesTheUpdate()
    {
        // Arrange
        var userId = AddUser();

        await _dbContext.SaveChangesAsync();

        var request = new UpdateUserSettingsRequest(false, true, "dark");

        // Act
        var result = await _userSettingsService.UpdateAsync(UserId.From(userId), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReceiveEmailNotifications.Should().BeFalse();
        result.Value.PreferredTheme.Should().Be("dark");
        (await _dbContext.UserSettings.CountAsync()).Should().Be(1);
    }

    [TestMethod]
    public async Task UpdateAsync_UnknownUser_ReturnsNotFound()
    {
        // Act
        var result = await _userSettingsService.UpdateAsync(UserId.From(Guid.NewGuid()), new UpdateUserSettingsRequest(true, true, null));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("UserSettings.NotFound");
    }

    private Guid AddUser()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "settings@example.com", FirstName = "Set", LastName = "Tings" };

        _dbContext.Users.Add(user);

        return user.Id;
    }
}
