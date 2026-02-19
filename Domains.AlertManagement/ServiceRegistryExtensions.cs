using AlertOne.Shared.Handlers;

namespace Domains.AlertManagement;

/// <summary>
/// Extension methods for registering AlertManagement domain services
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds AlertManagement domain services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddAlertManagement(this IServiceCollection services)
    {
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAlertTypeService, AlertTypeService>();
        services.AddScoped<IAlertStatusLevelService, AlertStatusLevelService>();

        services
            .AddHttpClient<AlertManagementApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://alerts-api");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return services;
    }
}
