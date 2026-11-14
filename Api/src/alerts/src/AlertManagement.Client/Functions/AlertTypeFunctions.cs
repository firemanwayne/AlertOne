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
/// Azure Functions for alert type management operations
/// </summary>
public sealed class AlertTypeFunctions(IAlertTypeService alertTypeService, ILogger<AlertTypeFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for listing alert types.
    /// </summary>
    public const string GET = "alert-types";
    /// <summary>
    /// Route for creating an alert type.
    /// </summary>
    public const string POST = GET;
    /// <summary>
    /// Route for getting an alert type by identifier.
    /// </summary>
    public const string GET_BY_ID = "alert-types/{id:guid}";
    /// <summary>
    /// Route for updating an alert type.
    /// </summary>
    public const string PUT = GET_BY_ID;
    /// <summary>
    /// Route for deleting an alert type.
    /// </summary>
    public const string DELETE = GET_BY_ID;

    /// <summary>
    /// Returns all alert types.
    /// </summary>
    [OpenApiOperation(operationId: "GetAllAlertTypes", tags: new[] { "Alert Types" }, Summary = "Returns all alert types.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<global::AlertManagement.Core.DTOs.Responses.AlertTypeResponse>), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("GetAllAlertTypes")]
    public async Task<HttpResponseData> GetAllAlertTypes([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all alert types");

        var result = await alertTypeService.GetAllAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve alert types");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns an alert type by identifier.
    /// </summary>
    [OpenApiOperation(operationId: "GetAlertTypeById", tags: new[] { "Alert Types" }, Summary = "Returns an alert type by identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertTypeResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("GetAlertTypeById")]
    public async Task<HttpResponseData> GetAlertTypeById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting alert type {AlertTypeId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertType.InvalidId", "Invalid alert type ID format");
        }

        var result = await alertTypeService.GetByIdAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve alert type")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Creates an alert type.
    /// </summary>
    [OpenApiOperation(operationId: "CreateAlertType", tags: new[] { "Alert Types" }, Summary = "Creates an alert type.")]
    [OpenApiRequestBody("application/json", typeof(global::AlertManagement.Core.DTOs.Requests.CreateAlertTypeRequest), Required = true, Description = "The create alert type request.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertTypeResponse), Description = "Created.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("CreateAlertType")]
    public async Task<HttpResponseData> CreateAlertType([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new alert type");

        var request = await req.ReadFromJsonAsync<CreateAlertTypeRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertTypeService.CreateAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create alert type")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Updates an alert type.
    /// </summary>
    [OpenApiOperation(operationId: "UpdateAlertType", tags: new[] { "Alert Types" }, Summary = "Updates an alert type.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiRequestBody("application/json", typeof(global::AlertManagement.Core.DTOs.Requests.UpdateAlertTypeRequest), Required = true, Description = "The update alert type request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.AlertTypeResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("UpdateAlertType")]
    public async Task<HttpResponseData> UpdateAlertType([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = PUT)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating alert type {AlertTypeId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertType.InvalidId", "Invalid alert type ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateAlertTypeRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertTypeService.UpdateAsync(id, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update alert type")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Deletes an alert type.
    /// </summary>
    [OpenApiOperation(operationId: "DeleteAlertType", tags: new[] { "Alert Types" }, Summary = "Deletes an alert type.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success, no content.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::AlertManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("DeleteAlertType")]
    public async Task<HttpResponseData> DeleteAlertType([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Deleting alert type {AlertTypeId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("AlertType.InvalidId", "Invalid alert type ID format");
        }

        var result = await alertTypeService.DeleteAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to delete alert type")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
