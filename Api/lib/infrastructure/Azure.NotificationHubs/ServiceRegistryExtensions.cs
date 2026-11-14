using AlertOne.Api.Core.Interfaces;
using Azure.NotificationHubs.Options;
using Azure.NotificationHubs.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azure.NotificationHubs;

/// <summary>
/// Extension methods for registering Azure Notification Hubs services.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds Azure Notification Hubs services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureNotificationHubs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureNotificationHubsOptions>(configuration.GetSection(AzureNotificationHubsOptions.SectionName));

        var connectionString = configuration[$"{AzureNotificationHubsOptions.SectionName}:{nameof(AzureNotificationHubsOptions.ConnectionString)}"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // No Notification Hub configured (local development): log instead of sending
            services.TryAddSingleton<IPushNotificationService, LoggingPushNotificationService>();

            return services;
        }

        services.TryAddSingleton<IPushNotificationService, PushNotificationService>();

        return services;
    }

    /// <summary>
    /// Adds Azure Notification Hubs services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureNotificationHubs(this IServiceCollection services, Action<AzureNotificationHubsOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<IPushNotificationService, PushNotificationService>();

        return services;
    }
}
