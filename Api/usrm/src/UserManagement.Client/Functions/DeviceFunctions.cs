using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using UserManagement.Client.Extensions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for device/session management operations
/// </summary>
public sealed class DeviceFunctions(
    IDeviceService deviceService,
    ILogger<DeviceFunctions> logger)
{
    // Route constants
    public const string GET_DEVICES = "devices";
    public const string GET_DEVICE = "devices/{id:guid}";
    public const string DELETE_DEVICE = GET_DEVICE;
    public const string DELETE_ALL_DEVICES = "devices/all";
    public const string DELETE_ALL_EXCEPT = "devices/all-except/{id:guid}";
    public const string TRUST_DEVICE = "devices/{id:guid}/trust";
    public const string UPDATE_DEVICE_NAME = "devices/{id:guid}/name";
    public const string GET_DEVICES_FOR_NOTIFICATIONS = "users/{userId:guid}/devices/notifications";

    [Function("GetActiveDevices")]
    public async Task<HttpResponseData> GetActiveDevices([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_DEVICES)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting active devices for user");

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        var result = await deviceService.GetActiveDevicesAsync(userId, context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve devices");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetDevice")]
    public async Task<HttpResponseData> GetDevice([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_DEVICE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting device {DeviceId}", id);

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        if (!Guid.TryParse(id, out var deviceId))
        {
            return await req.CreateBadRequestResponseAsync("Device.InvalidId", "Invalid device ID format");
        }

        var result = await deviceService.GetDeviceByIdAsync(userId, deviceId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve device")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("RevokeDevice")]
    public async Task<HttpResponseData> RevokeDevice([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE_DEVICE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Revoking device {DeviceId}", id);

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        if (!Guid.TryParse(id, out var deviceId))
        {
            return await req.CreateBadRequestResponseAsync("Device.InvalidId", "Invalid device ID format");
        }

        var result = await deviceService.RevokeDeviceAsync(userId, deviceId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to revoke device")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("RevokeAllDevices")]
    public async Task<HttpResponseData> RevokeAllDevices([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE_ALL_DEVICES)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Revoking all devices for user");

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        var result = await deviceService.RevokeAllDevicesAsync(userId, context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to revoke devices");
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("RevokeAllDevicesExcept")]
    public async Task<HttpResponseData> RevokeAllDevicesExcept([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE_ALL_EXCEPT)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Revoking all devices except {DeviceId}", id);

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        if (!Guid.TryParse(id, out var deviceId))
        {
            return await req.CreateBadRequestResponseAsync("Device.InvalidId", "Invalid device ID format");
        }

        var result = await deviceService.RevokeAllDevicesExceptAsync(userId, deviceId, context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to revoke devices");
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("TrustDevice")]
    public async Task<HttpResponseData> TrustDevice([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = TRUST_DEVICE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Marking device {DeviceId} as trusted", id);

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        if (!Guid.TryParse(id, out var deviceId))
        {
            return await req.CreateBadRequestResponseAsync("Device.InvalidId", "Invalid device ID format");
        }

        var result = await deviceService.TrustDeviceAsync(userId, deviceId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to trust device")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("UpdateDeviceName")]
    public async Task<HttpResponseData> UpdateDeviceName([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = UPDATE_DEVICE_NAME)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating device name for {DeviceId}", id);

        var userId = GetUserIdFromContext(context);

        if (userId is null)
        {
            return await req.CreateUnauthorizedResponseAsync("Auth.Unauthorized", "User not authenticated");
        }

        if (!Guid.TryParse(id, out var deviceId))
        {
            return await req.CreateBadRequestResponseAsync("Device.InvalidId", "Invalid device ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateDeviceNameRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await deviceService.UpdateDeviceNameAsync(userId, deviceId, request.DeviceName, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update device name")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetDevicesForNotifications")]
    public async Task<HttpResponseData> GetDevicesForNotifications([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_DEVICES_FOR_NOTIFICATIONS)] HttpRequestData req, string userId, FunctionContext context)
    {
        logger.LogInformation("Getting devices for notifications for user {UserId}", userId);

        if (!Guid.TryParse(userId, out var guidUserId))
        {
            return await req.CreateBadRequestResponseAsync("User.InvalidId", "Invalid user ID format");
        }

        var userIdObj = UserId.From(guidUserId);
        var result = await deviceService.GetDevicesForNotificationsAsync(userIdObj, context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve devices for notifications");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    private static UserId? GetUserIdFromContext(FunctionContext context)
    {
        var claimsPrincipal = context.Features.Get<ClaimsPrincipal>();

        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        return UserId.From(userId);
    }
}
