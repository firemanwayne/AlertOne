using AlertManagement.Client.Functions;
using AlertManagement.Client.IntegrationTests.Fixtures;
using AlertManagement.Client.IntegrationTests.Helpers;
using AlertOne.Api.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;

namespace AlertManagement.Client.IntegrationTests.Functions;

/// <summary>
/// Integration tests for NotificationFunctions.
/// </summary>
[TestClass]
public sealed class NotificationFunctionsTests
{
    private AlertManagementTestFixture _fixture = null!;
    private NotificationFunctions _notificationFunctions = null!;
    private Mock<ILogger<NotificationFunctions>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _fixture = new AlertManagementTestFixture();
        _loggerMock = new Mock<ILogger<NotificationFunctions>>();
        _notificationFunctions = new NotificationFunctions(
            _fixture.PushNotificationServiceMock.Object,
            _loggerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _fixture.Dispose();
    }

    #region Test Endpoint Tests

    [TestMethod]
    public async Task Test_WithValidRequest_SendsNotificationToAllDevices()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Test Title",
            body = "Test Body"
        }, context);

        // Act
        var response = await _notificationFunctions.Test(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToAllAsync(
                "Test Title",
                "Test Body",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Test_WithUserId_SendsNotificationToSpecificUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "User Notification",
            body = "Hello User!",
            userId
        }, context);

        // Act
        var response = await _notificationFunctions.Test(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToUserAsync(
                userId,
                "User Notification",
                "Hello User!",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Test_WithTags_SendsNotificationToTags()
    {
        // Arrange
        var tags = new[] { "role:admin", "team:ops" };
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Tag Notification",
            body = "Hello Team!",
            tags
        }, context);

        // Act
        var response = await _notificationFunctions.Test(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToTagsAsync(
                It.Is<IEnumerable<string>>(t => t.SequenceEqual(tags)),
                "Tag Notification",
                "Hello Team!",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Test_WithNullBody_UsesDefaultValues()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest("{}", context);

        // Act
        var response = await _notificationFunctions.Test(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToAllAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Test_WhenServiceFails_ReturnsInternalServerError()
    {
        // Arrange
        _fixture.PushNotificationServiceMock
            .Setup(x => x.SendToAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Test",
            body = "Test Body"
        }, context);

        // Act
        var response = await _notificationFunctions.Test(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region SendToUser Tests

    [TestMethod]
    public async Task SendToUser_WithValidRequest_SendsNotification()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Alert",
            body = "You have a new alert!"
        }, context);

        // Act
        var response = await _notificationFunctions.SendToUser(request, userId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToUserAsync(
                userId,
                "Alert",
                "You have a new alert!",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SendToUser_WithMissingTitle_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            body = "Body only"
        }, context);

        // Act
        var response = await _notificationFunctions.SendToUser(request, userId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region SendToAll Tests

    [TestMethod]
    public async Task SendToAll_WithValidRequest_SendsNotificationToAllDevices()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Broadcast",
            body = "Important announcement!"
        }, context);

        // Act
        var response = await _notificationFunctions.SendToAll(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToAllAsync(
                "Broadcast",
                "Important announcement!",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SendToAll_WithDataPayload_IncludesDataInNotification()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Alert",
            body = "Check the app",
            data = new { alertId = "123", priority = "high" }
        }, context);

        // Act
        var response = await _notificationFunctions.SendToAll(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToAllAsync(
                "Alert",
                "Check the app",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SendToTags Tests

    [TestMethod]
    public async Task SendToTags_WithValidRequest_SendsNotificationToTags()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Team Alert",
            body = "Attention required",
            tags = new[] { "role:manager" }
        }, context);

        // Act
        var response = await _notificationFunctions.SendToTags(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToTagsAsync(
                It.Is<IEnumerable<string>>(t => t.Contains("role:manager")),
                "Team Alert",
                "Attention required",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SendToTags_WithNoTags_ReturnsBadRequest()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Alert",
            body = "No tags",
            tags = Array.Empty<string>()
        }, context);

        // Act
        var response = await _notificationFunctions.SendToTags(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task SendToTags_WithMultipleTags_SendsToAllTags()
    {
        // Arrange
        var tags = new[] { "location:hq", "shift:day", "role:responder" };
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(new
        {
            title = "Shift Alert",
            body = "Emergency response needed",
            tags
        }, context);

        // Act
        var response = await _notificationFunctions.SendToTags(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _fixture.PushNotificationServiceMock.Verify(
            x => x.SendToTagsAsync(
                It.Is<IEnumerable<string>>(t => t.Count() == 3),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Route Constant Tests

    [TestMethod]
    public void RouteConstants_AreDefinedCorrectly()
    {
        // Assert
        NotificationFunctions.TEST.Should().Be("notifications/test");
        NotificationFunctions.SEND_TO_USER.Should().Be("notifications/send/user/{userId}");
        NotificationFunctions.SEND_TO_ALL.Should().Be("notifications/send/all");
        NotificationFunctions.SEND_TO_TAGS.Should().Be("notifications/send/tags");
    }

    #endregion
}
