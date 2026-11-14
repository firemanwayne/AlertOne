using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagement.Core.Interfaces;
using UserManagement.Infrastructure.Configurations;
using UserManagement.Infrastructure.Identity;
using UserManagement.Infrastructure.Services;

namespace UserManagement.Infrastructure;

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
        // Configure database
        services.AddDatabase(configuration);

        // Configure Identity with custom stores for Cosmos DB
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Can be set to true for production

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddUserStore<CosmosUserStore>()
        .AddRoleStore<CosmosRoleStore>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddDefaultTokenProviders();

        // Configure JWT
        services.ConfigureJwtAuthentication(configuration);

        // Register application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        return services;
    }

    /// <summary>
    /// Registers the Cosmos DB context, using the account key (emulator) or managed identity (Azure).
    /// </summary>
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosEndpoint = configuration.GetValue<string>("CosmosDb:Endpoint");
        var cosmosDatabase = configuration.GetValue<string>("CosmosDb:DatabaseName");
        var cosmosAccountKey = configuration.GetValue<string>("CosmosDb:AccountKey");

        ArgumentException.ThrowIfNullOrWhiteSpace(cosmosEndpoint, "CosmosDb:Endpoint");
        ArgumentException.ThrowIfNullOrWhiteSpace(cosmosDatabase, "CosmosDb:DatabaseName");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Use account key for local development (Cosmos Emulator), managed identity for Azure
            if (!string.IsNullOrWhiteSpace(cosmosAccountKey))
            {
                // The Cosmos emulator only supports gateway mode and a single endpoint
                options.UseCosmos(
                    cosmosEndpoint,
                    cosmosAccountKey,
                    cosmosDatabase,
                    cosmosOptions => cosmosOptions
                        .ConnectionMode(ConnectionMode.Gateway)
                        .LimitToEndpoint());
            }
            else
            {
                options.UseCosmos(
                    cosmosEndpoint,
                    new DefaultAzureCredential(),
                    cosmosDatabase);
            }
        });

        return services;
    }

    /// <summary>
    /// Configures JWT bearer authentication and binds the JWT settings.
    /// </summary>
    private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var sectionName = $"{nameof(JwtSettings)}:usrm";

        var jwtSettings = configuration
            .GetSection(sectionName)
            .Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings not configured");

        services.Configure<JwtSettings>(configuration.GetSection(sectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
