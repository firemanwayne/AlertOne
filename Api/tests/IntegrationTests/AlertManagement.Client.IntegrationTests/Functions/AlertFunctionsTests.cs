using AlertManagement.Client.Functions;
using AlertManagement.Client.IntegrationTests.Helpers;
using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;
using AlertManagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;

namespace AlertManagement.Client.IntegrationTests.Functions;

/// <summary>
/// Integration tests for AlertFunctions.
/// </summary>
[TestClass]
public sealed class AlertFunctionsTests
{
    private Mock<IAlertService> _alertServiceMock = null!;
    private Mock<ILogger<AlertFunctions>> _loggerMock = null!;
    private AlertFunctions _alertFunctions = null!;

    [TestInitialize]
    public void Setup()
    {
        _alertServiceMock = new Mock<IAlertService>();
        _loggerMock = new Mock<ILogger<AlertFunctions>>();
        _alertFunctions = new AlertFunctions(_alertServiceMock.Object, _loggerMock.Object);
    }

    #region GetAllAlerts Tests

    [TestMethod]
    public async Task GetAllAlerts_WhenSuccessful_ReturnsOk()
    {
        // Arrange
        var alerts = new List<AlertResponse>
        {
            CreateTestAlertResponse(),
            CreateTestAlertResponse()
        };

        _alertServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<AlertResponse>>(alerts));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetAllAlerts(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _alertServiceMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAllAlerts_WhenServiceFails_ReturnsInternalServerError()
    {
        // Arrange
        _alertServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IEnumerable<AlertResponse>>(Error.Internal("Alert.Error", "Database error")));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetAllAlerts(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GetActiveAlert Tests

    [TestMethod]
    public async Task GetActiveAlert_WhenAlertExists_ReturnsOk()
    {
        // Arrange
        var alert = CreateTestAlertResponse();

        _alertServiceMock
            .Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AlertResponse?>(alert));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetActiveAlert(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetActiveAlert_WhenNoActiveAlert_ReturnsNoContent()
    {
        // Arrange
        _alertServiceMock
            .Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AlertResponse?>(null));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetActiveAlert(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region GetAlertById Tests

    [TestMethod]
    public async Task GetAlertById_WithValidId_ReturnsOk()
    {
        // Arrange
        var alertId = Guid.NewGuid().ToString();
        var alert = CreateTestAlertResponse();

        _alertServiceMock
            .Setup(x => x.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(alert));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetAlertById(request, alertId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetAlertById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetAlertById(request, "", context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task GetAlertById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid().ToString();

        _alertServiceMock
            .Setup(x => x.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AlertResponse>(Error.NotFound("Alert.NotFound", "Alert not found")));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.GetAlertById(request, alertId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CreateAlert Tests

    [TestMethod]
    public async Task CreateAlert_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var createRequest = new CreateAlertRequest(
            AlertTypeId: Guid.NewGuid().ToString(),
            CreatedBy: "test-user"
        );

        var alert = CreateTestAlertResponse();

        _alertServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(alert));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(createRequest, context);

        // Act
        var response = await _alertFunctions.CreateAlert(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [TestMethod]
    public async Task CreateAlert_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateAlertRequest(
            AlertTypeId: "",
            CreatedBy: "test-user"
        );

        _alertServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AlertResponse>(Error.Validation("Alert.TypeRequired", "Alert type is required")));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(createRequest, context);

        // Act
        var response = await _alertFunctions.CreateAlert(request, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdateAlertStatus Tests

    [TestMethod]
    public async Task UpdateAlertStatus_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var alertId = Guid.NewGuid().ToString();
        var updateRequest = new UpdateAlertStatusRequest(
            AlertStatusLevelId: Guid.NewGuid().ToString(),
            UpdatedBy: "test-user"
        );

        var alert = CreateTestAlertResponse();

        _alertServiceMock
            .Setup(x => x.UpdateStatusAsync(alertId, It.IsAny<UpdateAlertStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(alert));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(updateRequest, context);

        // Act
        var response = await _alertFunctions.UpdateAlertStatus(request, alertId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task UpdateAlertStatus_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateEmptyMockRequest(context);

        // Act
        var response = await _alertFunctions.UpdateAlertStatus(request, "", context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region RescindAlert Tests

    [TestMethod]
    public async Task RescindAlert_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var alertId = Guid.NewGuid().ToString();
        var rescindRequest = new RescindAlertRequest(RescindedBy: "test-user");

        _alertServiceMock
            .Setup(x => x.RescindAsync(alertId, It.IsAny<RescindAlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(rescindRequest, context);

        // Act
        var response = await _alertFunctions.RescindAlert(request, alertId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task RescindAlert_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid().ToString();
        var rescindRequest = new RescindAlertRequest(RescindedBy: "test-user");

        _alertServiceMock
            .Setup(x => x.RescindAsync(alertId, It.IsAny<RescindAlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Alert.NotFound", "Alert not found")));

        var context = HttpRequestDataHelper.CreateMockFunctionContext();
        var request = HttpRequestDataHelper.CreateMockRequest(rescindRequest, context);

        // Act
        var response = await _alertFunctions.RescindAlert(request, alertId, context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Route Constant Tests

    [TestMethod]
    public void RouteConstants_AreDefinedCorrectly()
    {
        // Assert
        AlertFunctions.GET.Should().Be("alerts");
        AlertFunctions.POST.Should().Be("alerts");
        AlertFunctions.GET_ACTIVE.Should().Be("alerts/active");
        AlertFunctions.GET_BY_ID.Should().Be("alerts/{id:guid}");
        AlertFunctions.GET_HISTORY.Should().Be("alerts/{id:guid}/history");
        AlertFunctions.UPDATE_STATUS.Should().Be("alerts/{id:guid}/status");
        AlertFunctions.RESCIND.Should().Be("alerts/{id:guid}/rescind");
    }

    #endregion

    #region Helper Methods

    private static AlertResponse CreateTestAlertResponse()
    {
        var alertType = new AlertTypeResponse(
            Id: Guid.NewGuid().ToString(),
            Name: "Emergency",
            Description: "Emergency alert",
            BackgroundColor: "#FF0000",
            TextColor: "#FFFFFF",
            NotifyUserIds: new List<string>(),
            CreatedDate: DateTime.UtcNow,
            UpdatedDate: DateTime.UtcNow
        );

        var statusLevel = new AlertStatusLevelResponse(
            Id: Guid.NewGuid().ToString(),
            Name: "Active",
            Description: "Active status",
            BackgroundColor: "#00FF00",
            TextColor: "#FFFFFF",
            SendNotifications: true,
            IsDefault: true,
            IsCanceled: false,
            CreatedDate: DateTime.UtcNow,
            UpdatedDate: DateTime.UtcNow
        );

        return new AlertResponse(
            Id: Guid.NewGuid().ToString(),
            Type: alertType,
            StatusLevel: statusLevel,
            CreatedBy: "test-user",
            UpdatedBy: null,
            CreatedDate: DateTime.UtcNow,
            UpdatedDate: DateTime.UtcNow,
            IsActive: true,
            RescindedAt: null,
            RescindedBy: null
        );
    }

    #endregion
}
