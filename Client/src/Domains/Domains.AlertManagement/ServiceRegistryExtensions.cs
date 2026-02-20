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
                client.BaseAddress = new Uri("https://alertone-alrtmg-dev-eastus2-001-a4bzesh5c0cwbjbw.eastus2-01.azurewebsites.net");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return services;
    }
}
