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

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for user invitation operations
/// </summary>
public sealed class InvitationFunctions(
    IInvitationService invitationService,
    ILogger<InvitationFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for listing invitations.
    /// </summary>
    public const string GET = "invitations";
    /// <summary>
    /// Route for creating an invitation.
    /// </summary>
    public const string POST = GET;
    /// <summary>
    /// Route for getting an invitation by identifier.
    /// </summary>
    public const string GET_BY_ID = "invitations/{id}";
    /// <summary>
    /// Route for deleting an invitation.
    /// </summary>
    public const string DELETE = GET_BY_ID;
    /// <summary>
    /// Route for resending an invitation.
    /// </summary>
    public const string RESEND = "invitations/{id}/resend";
    /// <summary>
    /// Route for accepting an invitation.
    /// </summary>
    public const string ACCEPT = "invitations/accept";
    /// <summary>
    /// Route for validating an invitation activation code.
    /// </summary>
    public const string VALIDATE = "invitations/validate/{code}";

    /// <summary>
    /// Returns all invitations.
    /// </summary>
    [OpenApiOperation(operationId: "GetAllInvitations", tags: new[] { "Invitations" }, Summary = "Returns all invitations.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<global::UserManagement.Core.DTOs.Responses.InvitationResponse>), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Creates an invitation and emails the activation code.
    /// </summary>
    [OpenApiOperation(operationId: "CreateInvitation", tags: new[] { "Invitations" }, Summary = "Creates an invitation and emails the activation code.")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.CreateInvitationRequest), Required = true, Description = "The create invitation request.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.InvitationResponse), Description = "Created.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Returns an invitation by identifier.
    /// </summary>
    [OpenApiOperation(operationId: "GetInvitation", tags: new[] { "Invitations" }, Summary = "Returns an invitation by identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.InvitationResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Deletes an invitation.
    /// </summary>
    [OpenApiOperation(operationId: "DeleteInvitation", tags: new[] { "Invitations" }, Summary = "Deletes an invitation.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success, no content.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Resends an invitation email.
    /// </summary>
    [OpenApiOperation(operationId: "ResendInvitation", tags: new[] { "Invitations" }, Summary = "Resends an invitation email.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id.")]
    [OpenApiRequestBody("application/json", typeof(ResendInvitationRequest), Required = true, Description = "The resend invitation request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.InvitationResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Validates an invitation activation code.
    /// </summary>
    [OpenApiOperation(operationId: "ValidateInvitationCode", tags: new[] { "Invitations" }, Summary = "Validates an invitation activation code.")]
    [OpenApiParameter(name: "code", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The code.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.InvitationResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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

    /// <summary>
    /// Accepts an invitation and creates the user.
    /// </summary>
    [OpenApiOperation(operationId: "AcceptInvitation", tags: new[] { "Invitations" }, Summary = "Accepts an invitation and creates the user.")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.AcceptInvitationRequest), Required = true, Description = "The accept invitation request.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.UserResponse), Description = "Created.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The resource was not found.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
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
