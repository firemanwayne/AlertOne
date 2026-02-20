using AlertManagement.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AlertManagement.Client.Functions;

/// <summary>
/// Azure Functions for health check operations
/// </summary>
public sealed class HealthFunctions(AlertDbContext dbContext, ILogger<HealthFunctions> logger)
{
    // Route constants
    public const string PING = "health/ping";
    public const string READY = "health/ready";
    public const string LIVE = "health/live";

    [Function("HealthPing")]
    public HttpResponseData Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = PING)] HttpRequestData req)
    {
        logger.LogDebug("Health ping requested");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("pong");

        return response;
    }

    [Function("HealthLive")]
    public HttpResponseData Live([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = LIVE)] HttpRequestData req)
    {
        logger.LogDebug("Liveness check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("alive");

        return response;
    }

    [Function("HealthReady")]
    public async Task<HttpResponseData> Ready([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = READY)] HttpRequestData req, FunctionContext context)
    {
        logger.LogDebug("Readiness check requested");

        var healthResult = new HealthCheckResult
        {
            Service = "AlertManagement.Api",
            Timestamp = DateTime.UtcNow,
            Checks = []
        };

        // Check database connectivity
        var dbCheck = await CheckDatabaseAsync(context.CancellationToken);

        healthResult.Checks.Add(dbCheck);
        healthResult.Status = healthResult.Checks.All(c => c.Status == HealthStatus.Healthy)
            ? HealthStatus.Healthy
            : HealthStatus.Unhealthy;

        var statusCode = healthResult.Status == HealthStatus.Healthy
            ? HttpStatusCode.OK
            : HttpStatusCode.ServiceUnavailable;

        var response = req.CreateResponse(statusCode);

        await response.WriteAsJsonAsync(healthResult);

        return response;
    }

    private async Task<HealthCheck> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var check = new HealthCheck { Name = "Database" };

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                check.Status = HealthStatus.Healthy;
                check.Description = "Database connection successful";
            }
            else
            {
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Unable to connect to database";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");

            check.Status = HealthStatus.Unhealthy;
            check.Description = $"Database connection failed: {ex.Message}";
        }

        return check;
    }

    private sealed class HealthCheckResult
    {
        public required string Service { get; set; }
        public HealthStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public List<HealthCheck> Checks { get; set; } = [];
    }

    private sealed class HealthCheck
    {
        public required string Name { get; set; }
        public HealthStatus Status { get; set; }
        public string? Description { get; set; }
    }
}
