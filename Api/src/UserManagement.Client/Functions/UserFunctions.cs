using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using UserManagement.Client.Extensions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for user management operations
/// </summary>
public sealed class UserFunctions(
    IUserService userService,
    ILogger<UserFunctions> logger)
{
    // Route constants
    public const string GET = "users";
    public const string POST = GET;
    public const string GET_BY_ID = "users/{id:guid}";
    public const string PUT = GET_BY_ID;
    public const string DELETE = GET_BY_ID;

    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new user");

        var request = await req.ReadFromJsonAsync<CreateUserRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await userService.CreateUserAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create user")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all users");

        var result = await userService.GetAllUsersAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve users");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetUser")]
    public async Task<HttpResponseData> GetUser([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting user {UserId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("User.InvalidId", "Invalid user ID format");
        }

        var userId = UserId.From(guidId);
        var result = await userService.GetUserByIdAsync(userId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve user")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("UpdateUser")]
    public async Task<HttpResponseData> UpdateUser([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = PUT)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating user {UserId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("User.InvalidId", "Invalid user ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateUserRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var userId = UserId.From(guidId);

        var result = await userService.UpdateUserAsync(userId, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update user")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("DeleteUser")]
    public async Task<HttpResponseData> DeleteUser([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Deleting user {UserId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("User.InvalidId", "Invalid user ID format");
        }

        var userId = UserId.From(guidId);
        var result = await userService.DeleteUserAsync(userId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to delete user")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
