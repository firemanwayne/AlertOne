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

namespace UserManagement.Infrastructure.Extensions;

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

        // Configure Identity
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
        .AddEntityFrameworkStores<ApplicationDbContext>()
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

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = configuration
            .GetSection(nameof(DatabaseSettings))
            .Get<DatabaseSettings>() ?? throw new InvalidOperationException("DatabaseSettings not configured");

        services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (dbSettings.Provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(
                    dbSettings.ConnectionString,
                    b => b.MigrationsAssembly("UserManagement.Infrastructure"));
            }
            else
            {
                options.UseSqlServer(
                    dbSettings.ConnectionString,
                    b => b.MigrationsAssembly("UserManagement.Infrastructure"));
            }
        });

        return services;
    }

    private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection(nameof(JwtSettings))
            .Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings not configured");

        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

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
