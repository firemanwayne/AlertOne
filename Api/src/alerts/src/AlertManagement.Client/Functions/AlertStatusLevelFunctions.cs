using AlertManagement.Client.Extensions;
using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlertManagement.Client.Functions;

/// <summary>
/// Azure Functions for alert status level management operations
/// </summary>
public sealed class AlertStatusLevelFunctions(IAlertStatusLevelService alertStatusLevelService, ILogger<AlertStatusLevelFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for listing alert status levels.
    /// </summary>
    public const string GET = "alert-status-levels";
    /// <summary>
    /// Route for creating an alert status level.
    /// </summary>
    public const string POST = GET;
    /// <summary>
    /// Route for getting an alert status level by identifier.
    /// </summary>
    public const string GET_BY_ID = "alert-status-levels/{id:guid}";
    /// <summary>
    /// Route for updating an alert status level.
    /// </summary>
    public const string PUT = GET_BY_ID;
    /// <summary>
    /// Route for deleting an alert status level.
    /// </summary>
    public const string DELETE = GET_BY_ID;

    /// <summary>
    /// Returns all alert status levels.
    /// </summary>
    [OpenApiOperation(operationId: "GetAllAlertStatusLevels", tags: new[] { "Alert Status Levels" }, Summary = "Returns all alert status levels.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<global::AlertManagement.Core.DTOs.Responses.AlertStatusLevelResponse>), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("GetAllAlertStatusLevels")]
    public async Task<HttpResponseData> GetAllAlertStatusLevels([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all alert status levels");

        var result = await alertStatusLevelService.GetAllAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve alert status levels");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns an alert status level by identifier.
    /// </summary>
    [OpenApiOperation(operationId: "GetAlertStatusLevelById", tags: new[] { "Alert Status Levels" }, Summary = "Returns an alert status level by identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertStatusLevelResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("GetAlertStatusLevelById")]
    public async Task<HttpResponseData> GetAlertStatusLevelById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting alert status level {AlertStatusLevelId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertStatusLevel.InvalidId", "Invalid alert status level ID format");
        }

        var result = await alertStatusLevelService.GetByIdAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve alert status level")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Creates an alert status level.
    /// </summary>
    [OpenApiOperation(operationId: "CreateAlertStatusLevel", tags: new[] { "Alert Status Levels" }, Summary = "Creates an alert status level.")]
    [OpenApiRequestBody("application/json", typeof(global::AlertManagement.Core.DTOs.Requests.CreateAlertStatusLevelRequest), Required = true, Description = "The create alert status level request.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertStatusLevelResponse), Description = "Created.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("CreateAlertStatusLevel")]
    public async Task<HttpResponseData> CreateAlertStatusLevel([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new alert status level");

        var request = await req.ReadFromJsonAsync<CreateAlertStatusLevelRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertStatusLevelService.CreateAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create alert status level")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Updates an alert status level.
    /// </summary>
    [OpenApiOperation(operationId: "UpdateAlertStatusLevel", tags: new[] { "Alert Status Levels" }, Summary = "Updates an alert status level.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiRequestBody("application/json", typeof(global::AlertManagement.Core.DTOs.Requests.UpdateAlertStatusLevelRequest), Required = true, Description = "The update alert status level request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertStatusLevelResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("UpdateAlertStatusLevel")]
    public async Task<HttpResponseData> UpdateAlertStatusLevel([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = PUT)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating alert status level {AlertStatusLevelId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertStatusLevel.InvalidId", "Invalid alert status level ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateAlertStatusLevelRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertStatusLevelService.UpdateAsync(id, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update alert status level")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Deletes an alert status level; fails with a conflict when alerts still use it.
    /// </summary>
    [OpenApiOperation(operationId: "DeleteAlertStatusLevel", tags: new[] { "Alert Status Levels" }, Summary = "Deletes an alert status level; fails with a conflict when alerts still use it.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success, no content.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("DeleteAlertStatusLevel")]
    public async Task<HttpResponseData> DeleteAlertStatusLevel([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Deleting alert status level {AlertStatusLevelId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertStatusLevel.InvalidId", "Invalid alert status level ID format");
        }

        var result = await alertStatusLevelService.DeleteAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to delete alert status level")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
