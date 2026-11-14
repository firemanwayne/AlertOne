using AlertManagement.Client.Middleware;
using AlertManagement.Infrastructure;
using AlertManagement.Infrastructure.Data;
using Azure.AppConfig;
using Azure.CosmosDb;
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

// Create the Cosmos DB database/containers if missing (runs in the background, retries until Cosmos DB is ready)
builder
    .Services
    .AddCosmosInitialization<AlertDbContext>();

builder
    .UseMiddleware<ExceptionHandlingMiddleware>();

var host = builder.Build();

await host
    .RunAsync();
