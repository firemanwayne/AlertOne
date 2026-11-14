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
/// Azure Functions for alert management operations
/// </summary>
public sealed class AlertFunctions(IAlertService alertService, ILogger<AlertFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for listing alerts.
    /// </summary>
    public const string GET = "alerts";
    /// <summary>
    /// Route for creating an alert.
    /// </summary>
    public const string POST = GET;
    /// <summary>
    /// Route for getting the currently active alert.
    /// </summary>
    public const string GET_ACTIVE = "alerts/active";
    /// <summary>
    /// Route for getting an alert by identifier.
    /// </summary>
    public const string GET_BY_ID = "alerts/{id:guid}";
    /// <summary>
    /// Route for getting the history of an alert.
    /// </summary>
    public const string GET_HISTORY = "alerts/{id:guid}/history";
    /// <summary>
    /// Route for changing the status level of an alert.
    /// </summary>
    public const string UPDATE_STATUS = "alerts/{id:guid}/status";
    /// <summary>
    /// Route for rescinding an alert.
    /// </summary>
    public const string RESCIND = "alerts/{id:guid}/rescind";

    /// <summary>
    /// Returns all alerts.
    /// </summary>
    [Function("GetAllAlerts")]
    public async Task<HttpResponseData> GetAllAlerts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all alerts");

        var result = await alertService.GetAllAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve alerts");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns the currently active alert, if any.
    /// </summary>
    [Function("GetActiveAlert")]
    public async Task<HttpResponseData> GetActiveAlert([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_ACTIVE)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting active alert");

        var result = await alertService.GetActiveAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve active alert");
        }

        if (result.Value is null)
        {
            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns an alert by identifier.
    /// </summary>
    [Function("GetAlertById")]
    public async Task<HttpResponseData> GetAlertById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("Alert.InvalidId", "Invalid alert ID format");
        }

        var result = await alertService.GetByIdAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve alert")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns the event history of an alert.
    /// </summary>
    [Function("GetAlertHistory")]
    public async Task<HttpResponseData> GetAlertHistory([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_HISTORY)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting history for alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("Alert.InvalidId", "Invalid alert ID format");
        }

        var result = await alertService.GetHistoryAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve alert history")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Changes the status level of an alert.
    /// </summary>
    [Function("UpdateAlertStatus")]
    public async Task<HttpResponseData> UpdateAlertStatus([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = UPDATE_STATUS)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Updating status for alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("Alert.InvalidId", "Invalid alert ID format");
        }

        var request = await req.ReadFromJsonAsync<UpdateAlertStatusRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertService.UpdateStatusAsync(id, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to update alert status")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Creates a new alert.
    /// </summary>
    [Function("CreateAlert")]
    public async Task<HttpResponseData> CreateAlert([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new alert");

        var request = await req.ReadFromJsonAsync<CreateAlertRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertService.CreateAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create alert")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Rescinds (deactivates) an alert.
    /// </summary>
    [Function("RescindAlert")]
    public async Task<HttpResponseData> RescindAlert([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RESCIND)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Rescinding alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return await req.CreateBadRequestResponseAsync("Alert.InvalidId", "Invalid alert ID format");
        }

        var request = await req.ReadFromJsonAsync<RescindAlertRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await alertService.RescindAsync(id, request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to rescind alert")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
