using AlertOne.Shared.Blazor.Handlers;

using Domains.AlertManagement.Options;

using Microsoft.Extensions.Configuration;

namespace Domains.AlertManagement;

/// <summary>
/// Extension methods for registering AlertManagement domain services
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds AlertManagement domain services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddAlertManagement(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind options from configuration
        services.Configure<AlertManagementOptions>(configuration.GetSection("Api:AlertManagement"));

        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAlertTypeService, AlertTypeService>();
        services.AddScoped<IAlertStatusLevelService, AlertStatusLevelService>();

        services
            .AddHttpClient<IAlertManagementApiClient, AlertManagementApiClient>((sp, client) =>
            {
                var options = configuration
                .GetSection("Api:AlertManagement")
                .Get<AlertManagementOptions>();

                var baseUri = options?.BaseUri;

#if DEBUG
                // Use local development URL if not configured
                baseUri ??= "http://localhost:7072";
#endif

                if (!string.IsNullOrWhiteSpace(baseUri))
                {
                    client.BaseAddress = new Uri(baseUri);
                }

                // Add function key header if configured
                if (!string.IsNullOrWhiteSpace(options?.FunctionKey))
                {
                    client.DefaultRequestHeaders.Add("x-functions-key", options.FunctionKey);
                }

                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return services;
    }
}
