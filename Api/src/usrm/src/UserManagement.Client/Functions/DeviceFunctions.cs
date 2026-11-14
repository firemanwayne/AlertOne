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
    /// <summary>
    /// Route for listing the caller's active devices.
    /// </summary>
    public const string GET_DEVICES = "devices";
    /// <summary>
    /// Route for getting a device by identifier.
    /// </summary>
    public const string GET_DEVICE = "devices/{id:guid}";
    /// <summary>
    /// Route for revoking a device.
    /// </summary>
    public const string DELETE_DEVICE = GET_DEVICE;
    /// <summary>
    /// Route for revoking all of the caller's devices.
    /// </summary>
    public const string DELETE_ALL_DEVICES = "devices/all";
    /// <summary>
    /// Route for revoking all devices except one.
    /// </summary>
    public const string DELETE_ALL_EXCEPT = "devices/all-except/{id:guid}";
    /// <summary>
    /// Route for trusting a device.
    /// </summary>
    public const string TRUST_DEVICE = "devices/{id:guid}/trust";
    /// <summary>
    /// Route for renaming a device.
    /// </summary>
    public const string UPDATE_DEVICE_NAME = "devices/{id:guid}/name";
    /// <summary>
    /// Route for getting a user's devices that can receive notifications.
    /// </summary>
    public const string GET_DEVICES_FOR_NOTIFICATIONS = "users/{userId:guid}/devices/notifications";

    /// <summary>
    /// Returns the caller's active devices.
    /// </summary>
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

    /// <summary>
    /// Returns a device by identifier.
    /// </summary>
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

    /// <summary>
    /// Revokes a device.
    /// </summary>
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

    /// <summary>
    /// Revokes all of the caller's devices.
    /// </summary>
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

    /// <summary>
    /// Revokes all of the caller's devices except the given one.
    /// </summary>
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

    /// <summary>
    /// Marks a device as trusted.
    /// </summary>
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

    /// <summary>
    /// Renames a device.
    /// </summary>
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

    /// <summary>
    /// Returns the devices of a user that can receive push notifications.
    /// </summary>
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

    /// <summary>
    /// Gets the authenticated user's identifier from the name identifier claim, or null when it is missing or invalid.
    /// </summary>
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
