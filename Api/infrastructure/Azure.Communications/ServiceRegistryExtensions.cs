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
    public static IServiceCollection AddAzureCommunications(this IServiceCollection services, Action<AzureCommunicationsOptions> config)
    {
        services.Configure(config);

        services.AddSingleton<IEmailService, AzureEmailService>();

        return services;
    }
}
