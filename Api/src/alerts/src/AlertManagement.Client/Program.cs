using AlertManagement.Client.Middleware;
using AlertManagement.Infrastructure;
using AlertManagement.Infrastructure.Data;
using Azure.AppConfig;
using Azure.NotificationHubs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
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

// Apply database migrations on startup
await ApplyMigrationsAsync(host);

await host
    .RunAsync();

static async Task ApplyMigrationsAsync(IHost host)
{
    using var scope = host.Services.CreateScope();

    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");

        var context = services.GetRequiredService<AlertDbContext>();

        await context
            .Database
            .MigrateAsync();

        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw;
    }
}
