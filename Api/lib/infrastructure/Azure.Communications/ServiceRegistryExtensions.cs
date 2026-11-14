using AlertOne.Api.Core.Interfaces;
using Azure.Communications.Options;
using Azure.Communications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azure.Communications;

/// <summary>
/// Extension methods for registering Azure Communications services.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds Azure Communications services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureCommunications(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureCommunicationsOptions>(configuration.GetSection(AzureCommunicationsOptions.SectionName));

        var connectionString = configuration[$"{AzureCommunicationsOptions.SectionName}:{nameof(AzureCommunicationsOptions.ConnectionString)}"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // No Communication Services configured (local development): log instead of sending
            services.TryAddSingleton<IEmailService, LoggingEmailService>();

            return services;
        }

        services.TryAddSingleton<IEmailService, AzureEmailService>();

        return services;
    }
}
