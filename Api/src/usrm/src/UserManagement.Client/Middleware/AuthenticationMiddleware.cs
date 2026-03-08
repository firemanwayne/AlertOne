using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace UserManagement.Client.Middleware;

/// <summary>
/// Middleware for handling JWT authentication
/// </summary>
public sealed class AuthenticationMiddleware(ILogger<AuthenticationMiddleware> logger) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpRequestData = await context.GetHttpRequestDataAsync();

        if (httpRequestData is not null)
        {
            // Extract and log JWT token if present
            if (httpRequestData.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                var authHeader = authHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader["Bearer ".Length..].Trim();
                    logger.LogDebug("JWT token received for function {FunctionName}", context.FunctionDefinition.Name);

                    // Token validation is handled by JWT middleware configured in Program.cs
                    // The claims principal will be available via context
                }
            }
        }

        await next(context);
    }
}
