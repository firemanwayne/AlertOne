using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using UserManagement.Client.Extensions;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.Interfaces;

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for user invitation operations
/// </summary>
public sealed class InvitationFunctions(IInvitationService invitationService, ILogger<InvitationFunctions> logger)
{
    // Route constants
    public const string GET = "invitations";
    public const string POST = GET;
    public const string GET_BY_ID = "invitations/{id}";
    public const string DELETE = GET_BY_ID;
    public const string RESEND = "invitations/{id}/resend";
    public const string ACCEPT = "invitations/accept";
    public const string VALIDATE = "invitations/validate/{code}";

    [Function("GetAllInvitations")]
    public async Task<HttpResponseData> GetAllInvitations([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Getting all invitations");

        var result = await invitationService.GetAllAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            return await req.CreateInternalServerErrorResponseAsync(result.Error!.Code, "Failed to retrieve invitations");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("CreateInvitation")]
    public async Task<HttpResponseData> CreateInvitation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = POST)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Creating new invitation");

        var request = await req.ReadFromJsonAsync<CreateInvitationRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await invitationService.CreateAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to create invitation")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("GetInvitation")]
    public async Task<HttpResponseData> GetInvitation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GET_BY_ID)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Getting invitation {InvitationId}", id);

        var result = await invitationService.GetByIdAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to retrieve invitation")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("DeleteInvitation")]
    public async Task<HttpResponseData> DeleteInvitation([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = DELETE)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Deleting invitation {InvitationId}", id);

        var result = await invitationService.DeleteAsync(id, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to delete invitation")
            };
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("ResendInvitation")]
    public async Task<HttpResponseData> ResendInvitation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RESEND)] HttpRequestData req, string id, FunctionContext context)
    {
        logger.LogInformation("Resending invitation {InvitationId}", id);

        // Read resend request to get the resendBy field
        var resendRequest = await req.ReadFromJsonAsync<ResendInvitationRequest>();

        if (resendRequest is null || string.IsNullOrWhiteSpace(resendRequest.ResendBy))
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "ResendBy field is required");
        }

        var result = await invitationService.ResendAsync(id, resendRequest.ResendBy, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to resend invitation")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("ValidateInvitationCode")]
    public async Task<HttpResponseData> ValidateInvitationCode([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = VALIDATE)] HttpRequestData req, string code, FunctionContext context)
    {
        logger.LogInformation("Validating invitation code");

        var result = await invitationService.ValidateCodeAsync(code, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to validate invitation code")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    [Function("AcceptInvitation")]
    public async Task<HttpResponseData> AcceptInvitation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ACCEPT)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Accepting invitation");

        var request = await req.ReadFromJsonAsync<AcceptInvitationRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await invitationService.AcceptAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => await req.CreateNotFoundResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Failed to accept invitation")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }
}

/// <summary>
/// Request to resend an invitation
/// </summary>
public sealed record ResendInvitationRequest(string ResendBy);
