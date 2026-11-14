using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Client.Functions;

/// <summary>
/// Azure Functions for health check operations
/// </summary>
public sealed class HealthFunctions(ApplicationDbContext dbContext, ILogger<HealthFunctions> logger)
{
    // Route constants
    /// <summary>
    /// Route for the simple ping check.
    /// </summary>
    public const string PING = "health/ping";
    /// <summary>
    /// Route for the readiness check (verifies dependencies).
    /// </summary>
    public const string READY = "health/ready";
    /// <summary>
    /// Route for the liveness check.
    /// </summary>
    public const string LIVE = "health/live";

    /// <summary>
    /// Returns a simple response showing the host is running.
    /// </summary>
    [OpenApiOperation(operationId: "HealthPing", tags: new[] { "Health" }, Summary = "Returns a simple response showing the host is running.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "pong")]
    [Function("HealthPing")]
    public HttpResponseData Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = PING)] HttpRequestData req)
    {
        logger.LogDebug("Health ping requested");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("pong");

        return response;
    }

    /// <summary>
    /// Liveness probe: returns healthy while the host process is running.
    /// </summary>
    [OpenApiOperation(operationId: "HealthLive", tags: new[] { "Health" }, Summary = "Liveness probe: returns healthy while the host process is running.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "alive")]
    [Function("HealthLive")]
    public HttpResponseData Live([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = LIVE)] HttpRequestData req)
    {
        logger.LogDebug("Liveness check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("alive");

        return response;
    }

    /// <summary>
    /// Readiness probe: verifies dependencies such as the database.
    /// </summary>
    [OpenApiOperation(operationId: "HealthReady", tags: new[] { "Health" }, Summary = "Readiness probe: verifies dependencies such as the database.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HealthCheckResult), Description = "All dependencies are healthy.")]
    [OpenApiResponseWithBody(HttpStatusCode.ServiceUnavailable, "application/json", typeof(HealthCheckResult), Description = "A dependency is not healthy.")]
    [Function("HealthReady")]
    public async Task<HttpResponseData> Ready([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = READY)] HttpRequestData req, FunctionContext context)
    {
        logger.LogDebug("Readiness check requested");

        var healthResult = new HealthCheckResult
        {
            Service = "UserManagement.Api",
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

    /// <summary>
    /// Checks that the database can be reached.
    /// </summary>
    private async Task<HealthCheck> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var check = new HealthCheck { Name = "Database" };

        try
        {
            // The Cosmos provider does not support CanConnectAsync, so read the account properties instead
            var account = await dbContext
                .Database
                .GetCosmosClient()
                .ReadAccountAsync()
                .WaitAsync(cancellationToken);

            var canConnect = account is not null;

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

    /// <summary>
    /// Aggregated readiness result returned by the ready endpoint.
    /// </summary>
    private sealed class HealthCheckResult
    {
        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public required string Service { get; set; }
        /// <summary>
        /// Gets or sets the overall status.
        /// </summary>
        public HealthStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the UTC time the check ran.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Gets or sets the individual dependency checks.
        /// </summary>
        public List<HealthCheck> Checks { get; set; } = [];
    }

    /// <summary>
    /// Result of one dependency check.
    /// </summary>
    private sealed class HealthCheck
    {
        /// <summary>
        /// Gets or sets the check name.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the check status.
        /// </summary>
        public HealthStatus Status { get; set; }
        /// <summary>
        /// Gets or sets an optional description or failure detail.
        /// </summary>
        public string? Description { get; set; }
    }
}
