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
/// Azure Functions for authentication operations
/// </summary>
public sealed class AuthFunctions(
    ILogger<AuthFunctions> logger,
    IAuthenticationService authService)
{
    // Route constants
    /// <summary>
    /// Route for registering a user.
    /// </summary>
    public const string REGISTER = "auth/register";
    /// <summary>
    /// Route for logging in.
    /// </summary>
    public const string LOGIN = "auth/login";
    /// <summary>
    /// Route for refreshing an access token.
    /// </summary>
    public const string REFRESH = "auth/refresh";
    /// <summary>
    /// Route for checking whether any user exists.
    /// </summary>
    public const string HAS_USERS = "auth/has-users";

    /// <summary>
    /// Registers a new user and returns tokens.
    /// </summary>
    [OpenApiOperation(operationId: "Register", tags: new[] { "Authentication" }, Summary = "Registers a new user and returns tokens.")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.RegisterRequest), Required = true, Description = "The register request.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.AuthenticationResponse), Description = "Created.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request conflicts with the current state.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("Register")]
    public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = REGISTER)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Processing registration request");

        var request = await req.ReadFromJsonAsync<RegisterRequest>();
        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await authService.RegisterAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Validation => await req.CreateBadRequestResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Conflict => await req.CreateConflictResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Registration failed")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    [OpenApiOperation(operationId: "Login", tags: new[] { "Authentication" }, Summary = "Authenticates a user and returns tokens.")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.LoginRequest), Required = true, Description = "The login request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.AuthenticationResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The caller is not authenticated.")]
    [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The caller is not allowed to do this.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("Login")]
    public async Task<HttpResponseData> Login([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = LOGIN)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Processing login request");

        var request = await req.ReadFromJsonAsync<LoginRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await authService.LoginAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Unauthorized => await req.CreateUnauthorizedResponseAsync(result.Error.Code, result.Error.Message),
                ErrorType.Forbidden => await req.CreateForbiddenResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Login failed")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Exchanges a refresh token for new tokens.
    /// </summary>
    [OpenApiOperation(operationId: "RefreshToken", tags: new[] { "Authentication" }, Summary = "Exchanges a refresh token for new tokens.")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
    [OpenApiRequestBody("application/json", typeof(global::UserManagement.Core.DTOs.Requests.RefreshTokenRequest), Required = true, Description = "The refresh token request.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.AuthenticationResponse), Description = "Success.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The request is invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "The caller is not authenticated.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(global::UserManagement.Core.DTOs.Responses.ErrorResponse), Description = "An unexpected error occurred.")]
    [Function("RefreshToken")]
    public async Task<HttpResponseData> RefreshToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = REFRESH)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Processing token refresh request");

        var request = await req.ReadFromJsonAsync<RefreshTokenRequest>();

        if (request is null)
        {
            return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
        }

        var result = await authService.RefreshTokenAsync(request, context.CancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Unauthorized => await req.CreateUnauthorizedResponseAsync(result.Error.Code, result.Error.Message),
                _ => await req.CreateInternalServerErrorResponseAsync(result.Error.Code, "Token refresh failed")
            };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(result.Value);

        return response;
    }

    /// <summary>
    /// Returns whether at least one user exists (used for first-run setup).
    /// </summary>
    [OpenApiOperation(operationId: "HasUsers", tags: new[] { "Authentication" }, Summary = "Returns whether at least one user exists (used for first-run setup).")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Dictionary<string, bool>), Description = "An object with a hasUsers flag.")]
    [Function("HasUsers")]
    public async Task<HttpResponseData> HasUsers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = HAS_USERS)] HttpRequestData req, FunctionContext context)
    {
        logger.LogInformation("Checking if any users exist");

        var result = await authService.HasUsersAsync(context.CancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new { hasUsers = result.Value });

        return response;
    }
}
