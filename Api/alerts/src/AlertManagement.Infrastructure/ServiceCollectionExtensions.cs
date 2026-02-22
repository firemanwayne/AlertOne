using AlertManagement.Core.Interfaces;
using AlertManagement.Infrastructure.Data;
using AlertManagement.Infrastructure.Services;
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

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("SqlServer");

        services
            .AddDbContext<AlertDbContext>(options =>
        {
            options
            .UseAzureSql(
                connectionString,
                b => b.MigrationsAssembly("AlertManagement.Infrastructure"));

        });

        return services;
    }
}
