using System.Net;
using AlertManagement.Core.DTOs.Responses;
using Microsoft.Azure.Functions.Worker.Http;

namespace AlertManagement.Client.Extensions;

/// <summary>
/// Extension methods for HTTP request handling
/// </summary>
public static class HttpRequestExtensions
{
    public static async Task<HttpResponseData> CreateBadRequestResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }

    public static async Task<HttpResponseData> CreateUnauthorizedResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }

    public static async Task<HttpResponseData> CreateForbiddenResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Forbidden);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }

    public static async Task<HttpResponseData> CreateNotFoundResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.NotFound);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }

    public static async Task<HttpResponseData> CreateConflictResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Conflict);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }

    public static async Task<HttpResponseData> CreateInternalServerErrorResponseAsync(this HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        await response.WriteAsJsonAsync(new ErrorResponse(code, message));
        return response;
    }
}
