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
/// Azure Functions for authentication operations
/// </summary>
public sealed class AuthFunctions(
    ILogger<AuthFunctions> logger,
    IAuthenticationService authService)
{
    // Route constants
    public const string REGISTER = "auth/register";
    public const string LOGIN = "auth/login";
    public const string REFRESH = "auth/refresh";
    public const string HAS_USERS = "auth/has-users";

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
