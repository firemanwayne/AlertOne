using AlertOne.Shared.Handlers;

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
        services.AddScoped<IInvitationService, InvitationService>();

        // Register domain-specific ViewModels, handlers, or other services here
        // Example:
        // services.AddTransient<UserListViewModel>();
        // services.AddTransient<UserEditViewModel>();
        // services.AddTransient<RoleListViewModel>();

        services
            .AddHttpClient<UserManagementApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://alertone-usrm-dev-eastus2-001-hxhjhwdbethbgtgt.eastus2-01.azurewebsites.net");
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return services;
    }
}
