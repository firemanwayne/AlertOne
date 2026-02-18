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
/// Azure Functions for role management operations
/// </summary>
public sealed class RoleFunctions(
    IRoleService roleService,
    ILogger<RoleFunctions> logger)
{
    // Route constants
    public const string GET = "roles";
    public const string POST = GET;
    public const string GET_BY_ID = "roles/{id:guid}";
    public const string PUT = GET_BY_ID;
    public const string DELETE = GET_BY_ID;
    public const string GET_BY_NAME = "roles/name/{name}";
    public const string ASSIGN = "roles/assign";
    public const string REMOVE = "roles/remove";
    public const string GET_USERS_IN_ROLE = "roles/{name}/users";

    [Function("CreateRole")]
    public async Task<HttpResponseData> CreateRole([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new role");

        var request = await req.ReadFromJsonAsync<CreateRoleRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await roleService.CreateRoleAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetRoleById")]
    public async Task<HttpResponseData> GetRoleById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting role {RoleId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("Role.InvalidId", "Invalid role ID format");
        }

        var result = await roleService.GetRoleByIdAsync(guidId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetRoleByName")]
    public async Task<HttpResponseData> GetRoleByName([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_NAME)] HttpRequestData req, string name, FunctionContext context)
    {
        logger.LogInformation("Getting role by name: {RoleName}", name);

        var result = await roleService.GetRoleByNameAsync(name, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetAllRoles")]
    public async Task<HttpResponseData> GetAllRoles([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all roles");

        var result = await roleService.GetAllRolesAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve roles");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("UpdateRole")]
    public async Task<HttpResponseData> UpdateRole([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = PUT)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating role {RoleId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("Role.InvalidId", "Invalid role ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateRoleRequest>();
        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await roleService.UpdateRoleAsync(guidId, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result.Value);
        return response;
    }

    [Function("DeleteRole")]
    public async Task<HttpResponseData> DeleteRole([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Deleting role {RoleId}", id);

        if (!Guid.TryParse(id, out var guidId))
        {
            return await req.CreateBadRequestResponseAsync("Role.InvalidId", "Invalid role ID format");
        }

        var result = await roleService.DeleteRoleAsync(guidId, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to delete role")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("AssignRoleToUser")]
    public async Task<HttpResponseData> AssignRoleToUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ASSIGN)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Assigning role to user");

        var request = await req.ReadFromJsonAsync<AssignRoleRequest>();
        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var userId = UserId.From(request.UserId);
        var result = await roleService.AssignRoleToUserAsync(userId, request.RoleName, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to assign role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new { message = $"Role '{request.RoleName}' assigned successfully" });

        return response;
    }

    [Function("RemoveRoleFromUser")]
    public async Task<HttpResponseData> RemoveRoleFromUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = REMOVE)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Removing role from user");

        var request = await req.ReadFromJsonAsync<AssignRoleRequest>();
        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var userId = UserId.From(request.UserId);

        var result = await roleService.RemoveRoleFromUserAsync(userId, request.RoleName, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to remove role")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new { message = $"Role '{request.RoleName}' removed successfully" });

        return response;
    }

    [Function("GetUsersInRole")]
    public async Task<HttpResponseData> GetUsersInRole([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_USERS_IN_ROLE)] HttpRequestData req, string name, FunctionContext context)
    {
        logger.LogInformation("Getting users in role: {RoleName}", name);

        var result = await roleService.GetUsersInRoleAsync(name, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve users")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }
}
