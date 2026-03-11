using AlertOne.Shared.Handlers;

using Domains.UserManagement.Options;

using Microsoft.Extensions.Configuration;

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
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserManagement(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind options from configuration
        services.Configure<UserManagementOptions>(configuration.GetSection("Api:UserManagement"));

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        services
            .AddHttpClient<IUserManagementApiClient, UserManagementApiClient>((sp, client) =>
            {
                var options = configuration.GetSection("Api:UserManagement").Get<UserManagementOptions>();
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
