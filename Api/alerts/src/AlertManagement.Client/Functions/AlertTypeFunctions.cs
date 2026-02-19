using AlertManagement.Client.Extensions;
using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlertManagement.Client.Functions;

/// <summary>
/// Azure Functions for alert type management operations
/// </summary>
public sealed class AlertTypeFunctions(IAlertTypeService alertTypeService, ILogger<AlertTypeFunctions> logger)
{
    // Route constants
    public const string GET = "alert-types";
    public const string POST = GET;
    public const string GET_BY_ID = "alert-types/{id:guid}";
    public const string PUT = GET_BY_ID;
    public const string DELETE = GET_BY_ID;

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
