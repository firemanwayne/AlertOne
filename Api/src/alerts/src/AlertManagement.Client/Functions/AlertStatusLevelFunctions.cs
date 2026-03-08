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
/// Azure Functions for alert status level management operations
/// </summary>
public sealed class AlertStatusLevelFunctions(IAlertStatusLevelService alertStatusLevelService, ILogger<AlertStatusLevelFunctions> logger)
{
    // Route constants
    public const string GET = "alert-status-levels";
    public const string POST = GET;
    public const string GET_BY_ID = "alert-status-levels/{id:guid}";
    public const string PUT = GET_BY_ID;
    public const string DELETE = GET_BY_ID;

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
