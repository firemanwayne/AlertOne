using AlertOne.Shared.Blazor.Handlers;

using Domains.UserManagement.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        services.TryAddScoped<IAuthenticationService, AuthenticationService>();

        services.TryAddScoped<IUserManagementService, UserManagementService>();

        services.TryAddScoped<IRoleManagementService, RoleManagementService>();

        services.TryAddScoped<IInvitationService, InvitationService>();

        services.TryAddScoped<IUserSettingsService, UserSettingsService>();

        services
            .AddHttpClient<IUserManagementApiClient, UserManagementApiClient>((sp, client) =>
            {
                var options = configuration
                .GetSection("Api:UserManagement")
                .Get<UserManagementOptions>() ?? throw new InvalidCastException(nameof(UserManagementOptions));

                ArgumentException.ThrowIfNullOrWhiteSpace(options.BaseUri);

                //ArgumentException.ThrowIfNullOrWhiteSpace(options.FunctionKey);

                client.BaseAddress = new Uri(options.BaseUri);

                // Add function key header if configured
                if (!string.IsNullOrWhiteSpace(options.FunctionKey))
                {
                    client.DefaultRequestHeaders.Add("x-functions-key", options.FunctionKey);
                }

                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return services;
    }
}
