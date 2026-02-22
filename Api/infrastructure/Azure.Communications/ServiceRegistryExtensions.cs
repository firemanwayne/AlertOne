using AlertOne.Api.Core.Interfaces;
using Azure.Communications.Options;
using Azure.Communications.Services;
using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="config">The configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureCommunications(this IServiceCollection services, Func<IServiceProvider, AzureCommunicationsOptions> config)
    {
        services.Add(
            new ServiceDescriptor(typeof(AzureCommunicationsOptions),
            config.Invoke,
            ServiceLifetime.Singleton));

        services.AddSingleton<IEmailService, AzureEmailService>();

        return services;
    }
}
