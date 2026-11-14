using Azure.CosmosDb;
using Microsoft.Azure.Cosmos;
using AlertManagement.Core.Interfaces;
using AlertManagement.Infrastructure.Data;
using AlertManagement.Infrastructure.Services;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlertManagement.Infrastructure;

/// <summary>
/// Extension methods for service collection to configure infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services
            .AddScoped<IAlertService, AlertService>()
            .AddScoped<IAlertTypeService, AlertTypeService>()
            .AddScoped<IAlertStatusLevelService, AlertStatusLevelService>();

        return services;
    }

    /// <summary>
    /// Registers the Cosmos DB context, using the account key (emulator) or managed identity (Azure).
    /// </summary>
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosEndpoint = configuration.GetValue<string>("CosmosDb:Endpoint");
        var cosmosDatabase = configuration.GetValue<string>("CosmosDb:DatabaseName");
        var cosmosAccountKey = configuration.GetValue<string>("CosmosDb:AccountKey");

        ArgumentException.ThrowIfNullOrWhiteSpace(cosmosEndpoint, "CosmosDb:Endpoint");
        ArgumentException.ThrowIfNullOrWhiteSpace(cosmosDatabase, "CosmosDb:DatabaseName");

        services.AddDbContext<AlertDbContext>(options =>
        {
            // Use account key for local development (Cosmos Emulator), managed identity for Azure
            if (!string.IsNullOrWhiteSpace(cosmosAccountKey))
            {
                // The Cosmos emulator only supports gateway mode and a single endpoint, and its binary responses cannot be read, so ask for plain JSON
                options.UseCosmos(
                    cosmosEndpoint,
                    cosmosAccountKey,
                    cosmosDatabase,
                    cosmosOptions => cosmosOptions
                        .ConnectionMode(ConnectionMode.Gateway)
                        .LimitToEndpoint()
                        .HttpClientFactory(CosmosEmulatorHttpClient.Create));
            }
            else
            {
                options.UseCosmos(
                    cosmosEndpoint,
                    new DefaultAzureCredential(),
                    cosmosDatabase);
            }
        });

        return services;
    }
}
