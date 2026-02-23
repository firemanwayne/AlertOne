using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;
using AlertOne.Api.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlertManagement.Client.Functions;

/// <summary>
/// Azure Functions for push notification operations.
/// </summary>
public sealed class NotificationFunctions(IPushNotificationService pushNotificationService, ILogger<NotificationFunctions> logger)
{
    // Route constants
    public const string REGISTER = "notifications/register";
    public const string UNREGISTER = "notifications/unregister/{registrationId}";
    public const string SEND_TO_USER = "notifications/send/user/{userId}";
    public const string SEND_TO_ALL = "notifications/send/all";
    public const string SEND_TO_TAGS = "notifications/send/tags";
    public const string TEST = "notifications/test";

    /// <summary>
    /// Registers a device for push notifications.
    /// </summary>
    [Function("NotificationRegister")]
    public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = REGISTER)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Registering device for push notifications");

        var request = await req.ReadFromJsonAsync<RegisterDeviceRequest>();

        if (request is null || string.IsNullOrEmpty(request.DeviceToken) || string.IsNullOrEmpty(request.Platform) || string.IsNullOrEmpty(request.UserId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

            await badRequest.WriteAsJsonAsync(new { error = "DeviceToken, Platform, and UserId are required" });

            return badRequest;
        }

        try
        {
            var registrationId = await pushNotificationService.RegisterDeviceAsync(request.DeviceToken, request.Platform, request.UserId, request.Tags, context.CancellationToken);

            logger.LogInformation("Device registered successfully. RegistrationId: {RegistrationId}, UserId: {UserId}, Platform: {Platform}", registrationId, request.UserId, request.Platform);

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new DeviceRegistrationResponse(registrationId, request.Platform, DateTime.UtcNow));

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register device for user {UserId}", request.UserId);

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);

            await errorResponse.WriteAsJsonAsync(new { error = "Failed to register device", details = ex.Message });

            return errorResponse;
        }
    }

    /// <summary>
    /// Unregisters a device from push notifications.
    /// </summary>
    [Function("NotificationUnregister")]
    public async Task<HttpResponseData> Unregister([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = UNREGISTER)] HttpRequestData req, string registrationId, FunctionContext context)
    {
        logger.LogInformation("Unregistering device. RegistrationId: {RegistrationId}", registrationId);

        if (string.IsNullOrEmpty(registrationId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "RegistrationId is required" });
            return badRequest;
        }

        try
        {
            await pushNotificationService.UnregisterDeviceAsync(registrationId, context.CancellationToken);

            logger.LogInformation("Device unregistered successfully. RegistrationId: {RegistrationId}", registrationId);

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unregister device {RegistrationId}", registrationId);

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);

            await errorResponse.WriteAsJsonAsync(new { error = "Failed to unregister device", details = ex.Message });

            return errorResponse;
        }
    }

    /// <summary>
    /// Test endpoint to verify notification hub connectivity.
    /// </summary>
    [Function("NotificationTest")]
    public async Task<HttpResponseData> Test([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = TEST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Testing notification hub connectivity");

        try
        {
            var request = await req.ReadFromJsonAsync<TestNotificationRequest>();

            if (request is null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badRequest;
            }

            var title = request.Title ?? "Test Notification";
            var body = request.Body ?? "This is a test notification from AlertOne";
            var data = request.Data ?? new Dictionary<string, string> { { "test", "true" } };

            bool success;

            if (!string.IsNullOrEmpty(request.UserId))
            {
                logger.LogInformation("Sending test notification to user {UserId}", request.UserId);
                success = await pushNotificationService.SendToUserAsync(request.UserId, title, body, data, context.CancellationToken);
            }
            else if (request.Tags is not null && request.Tags.Any())
            {
                logger.LogInformation("Sending test notification to tags: {Tags}", string.Join(", ", request.Tags));
                success = await pushNotificationService.SendToTagsAsync(request.Tags, title, body, data, context.CancellationToken);
            }
            else
            {
                logger.LogInformation("Sending test notification to all devices");
                success = await pushNotificationService.SendToAllAsync(title, body, data, context.CancellationToken);
            }

            var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

            await response.WriteAsJsonAsync(new
            {
                success,
                message = success ? "Notification sent successfully" : "Failed to send notification",
                title,
                body,
                targetUserId = request.UserId,
                targetTags = request.Tags
            });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send test notification");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);

            await errorResponse.WriteAsJsonAsync(new
            {
                success = false,
                error = ex.Message
            });

            return errorResponse;
        }
    }

    /// <summary>
    /// Send notification to a specific user.
    /// </summary>
    [Function("NotificationSendToUser")]
    public async Task<HttpResponseData> SendToUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = SEND_TO_USER)] HttpRequestData req, string userId, FunctionContext context)
    {
        logger.LogInformation("Sending notification to user {UserId}", userId);

        var request = await req.ReadFromJsonAsync<SendNotificationRequest>();

        if (request is null || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Title and body are required" });
            return badRequest;
        }

        var success = await pushNotificationService.SendToUserAsync(userId, request.Title, request.Body, request.Data, context.CancellationToken);

        var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

        await response.WriteAsJsonAsync(new
        {
            success,
            message = success ? "Notification sent" : "Failed to send notification"
        });

        return response;
    }

    /// <summary>
    /// Send notification to all registered devices.
    /// </summary>
    [Function("NotificationSendToAll")]
    public async Task<HttpResponseData> SendToAll([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = SEND_TO_ALL)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Sending notification to all devices");

        var request = await req.ReadFromJsonAsync<SendNotificationRequest>();

        if (request is null || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Title and body are required" });
            return badRequest;
        }

        var success = await pushNotificationService.SendToAllAsync(request.Title, request.Body, request.Data, context.CancellationToken);

        var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

        await response.WriteAsJsonAsync(new
        {
            success,
            message = success ? "Notification sent to all devices" : "Failed to send notification"
        });

        return response;
    }

    /// <summary>
    /// Send notification to devices with specific tags.
    /// </summary>
    [Function("NotificationSendToTags")]
    public async Task<HttpResponseData> SendToTags([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = SEND_TO_TAGS)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Sending notification to tags");

        var request = await req.ReadFromJsonAsync<SendToTagsRequest>();

        if (request is null || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Title and body are required" });
            return badRequest;
        }

        if (request.Tags is null || !request.Tags.Any())
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "At least one tag is required" });
            return badRequest;
        }

        var success = await pushNotificationService.SendToTagsAsync(request.Tags, request.Title, request.Body, request.Data, context.CancellationToken);

        var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

        await response.WriteAsJsonAsync(new
        {
            success,
            message = success ? "Notification sent to tags" : "Failed to send notification",
            tags = request.Tags
        });

        return response;
    }

    private sealed class TestNotificationRequest
    {
        public string Title { get; set; } = "This is a test";
        public string? Body { get; set; } = "This is the test body";
        public string? UserId { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    private sealed class SendNotificationRequest
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    private sealed class SendToTagsRequest
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required IEnumerable<string> Tags { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }
}
