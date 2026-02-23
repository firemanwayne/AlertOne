using AlertOne.Api.Core.Interfaces;
using Azure.Communications.Options;
using Azure.Communications.Services;
using Microsoft.Extensions.Configuration;
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
    /// <param name="configuration">The configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureCommunications(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureCommunicationsOptions>(configuration.GetSection(AzureCommunicationsOptions.SectionName));

        services.AddSingleton<IEmailService, AzureEmailService>();

        return services;
    }
}
