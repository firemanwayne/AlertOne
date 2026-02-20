using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using UserManagement.Core.DTOs.Responses;

namespace UserManagement.Client.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions
/// </summary>
public sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred in function {FunctionName}", context.FunctionDefinition.Name);

            var httpRequestData = await context.GetHttpRequestDataAsync();
            if (httpRequestData is not null)
            {
                var response = httpRequestData.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new ErrorResponse(
                    "Internal.Error",
                    "An unexpected error occurred. Please try again later."
                ));

                var invocationResult = context.GetInvocationResult();
                invocationResult.Value = response;
            }
        }
    }
}
