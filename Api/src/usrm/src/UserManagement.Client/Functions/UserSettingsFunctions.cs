using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;
using UserManagement.Client.Extensions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for user settings operations
/// </summary>
public sealed class UserSettingsFunctions(IUserSettingsService userSettingsService, ILogger<UserSettingsFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for getting a user's settings.
    /// </summary>
    public const string GET_BY_USER_ID = "users/{userId:guid}/settings";
    /// <summary>
    /// Route for updating a user's settings.
    /// </summary>
    public const string PUT = GET_BY_USER_ID;

    /// <summary>
    /// Returns a user's settings.
    /// </summary>
    [OpenApiOperation(operationId: "GetUserSettings", tags: new[] { "User Settings" }, Summary = "Returns a user's settings.")]
    [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The user id.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.UserSettingsResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("GetUserSettings")]
    public async Task<HttpResponseData> GetUserSettings([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_USER_ID)] HttpRequestData req, string userId, FunctionContext context)
    {
        logger.LogInformation("Getting settings for user {UserId}", userId);

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return await req.CreateBadRequestResponseAsync("UserId.Invalid", "Invalid user ID format");
        }

        var result = await userSettingsService.GetByUserIdAsync(UserId.From(userGuid), context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve user settings")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Updates a user's settings.
    /// </summary>
    [OpenApiOperation(operationId: "UpdateUserSettings", tags: new[] { "User Settings" }, Summary = "Updates a user's settings.")]
    [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The user id.")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.UpdateUserSettingsRequest), Required = true, Description = "The update user settings request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.UserSettingsResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("UpdateUserSettings")]
    public async Task<HttpResponseData> UpdateUserSettings([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = PUT)] HttpRequestData req, string userId, FunctionContext context)
    {
        logger.LogInformation("Updating settings for user {UserId}", userId);

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return await req.CreateBadRequestResponseAsync("UserId.Invalid", "Invalid user ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateUserSettingsRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await userSettingsService.UpdateAsync(UserId.From(userGuid), request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update user settings")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }
}
