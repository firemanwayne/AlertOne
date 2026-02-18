using Domains.AlertManagement.Interfaces;
using Domains.AlertManagement.Services;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
