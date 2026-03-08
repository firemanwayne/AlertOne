using Azure.AppConfig;
using Azure.Communications;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserManagement.Client.Middleware;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Identity;

var builder = FunctionsApplication.CreateBuilder(args);

builder
    .Configuration
    .AddAppConfiguration();

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService();

// Add Infrastructure services (Identity, EF Core, JWT)
builder
    .Services
    .AddInfrastructure(builder.Configuration);

builder
    .Services
    .AddAzureCommunications(builder.Configuration);

builder
    .UseMiddleware<ExceptionHandlingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>();

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

        var context = services.GetRequiredService<ApplicationDbContext>();

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
