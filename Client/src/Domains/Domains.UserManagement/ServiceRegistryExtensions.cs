using Domains.UserManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using UserManagementApps.Core.Interfaces;

namespace Domains.UserManagement;

/// <summary>
/// Extension methods for registering UserManagement domain services
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Adds UserManagement domain services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="apiBaseUrl">The base URL for the UserManagement API</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserManagement(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();

        // Register domain-specific ViewModels, handlers, or other services here
        // Example:
        // services.AddTransient<UserListViewModel>();
        // services.AddTransient<UserEditViewModel>();
        // services.AddTransient<RoleListViewModel>();

        return services;
    }
}
