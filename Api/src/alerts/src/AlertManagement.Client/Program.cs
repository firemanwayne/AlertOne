using AlertManagement.Client.Middleware;
using AlertManagement.Infrastructure;
using AlertManagement.Infrastructure.Data;
using Azure.AppConfig;
using Azure.NotificationHubs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder
    .Configuration
    .AddAppConfiguration();

builder
    .Services
    .AddAzureNotificationHubs(builder.Configuration);

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService();

// Add Infrastructure services (EF Core, alert services)
builder
    .Services
    .AddInfrastructure(builder.Configuration);

builder
    .UseMiddleware<ExceptionHandlingMiddleware>();

var host = builder.Build();

// Ensure Cosmos DB database and containers exist
await EnsureDatabaseCreatedAsync(host);

await host
    .RunAsync();

static async Task EnsureDatabaseCreatedAsync(IHost host)
{
    using var scope = host.Services.CreateScope();

    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring Cosmos DB database and containers exist...");

        var context = services.GetRequiredService<AlertDbContext>();

        await context
            .Database
            .EnsureCreatedAsync();

        logger.LogInformation("Cosmos DB database and containers verified successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring Cosmos DB database exists");
        throw;
    }
}
