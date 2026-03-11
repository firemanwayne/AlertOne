using Azure.AppConfig;
using Azure.Communications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserManagement.Client.Middleware;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Identity;

var builder = FunctionsApplication.CreateBuilder(args);

builder
    .Configuration
    .AddAppConfiguration();

builder
    .Services
    .AddApplicationInsightsTelemetryWorkerService();

// Add Infrastructure services (Identity, EF Core, JWT)
builder
    .Services
    .AddInfrastructure(builder.Configuration);

builder
    .Services
    .AddAzureCommunications(builder.Configuration);

builder
    .UseMiddleware<ExceptionHandlingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>();

var host = builder.Build();

// Ensure Cosmos DB database and containers exist
await EnsureDatabaseCreatedAsync(host);

await host
    .RunAsync();

static async Task EnsureDatabaseCreatedAsync(IHost host)
{
    using var scope = host.Services.CreateScope();

    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring Cosmos DB database and containers exist...");

        var context = services.GetRequiredService<ApplicationDbContext>();

        await context
            .Database
            .EnsureCreatedAsync();

        logger.LogInformation("Cosmos DB database and containers verified successfully");

        // Seed default roles
        await SeedRolesAsync(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring Cosmos DB database exists");
        throw;
    }
}

static async Task SeedRolesAsync(IServiceProvider services, ILogger logger)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    string[] defaultRoles = ["Admin", "Manager", "User"];

    foreach (var roleName in defaultRoles)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            logger.LogInformation("Creating role: {RoleName}", roleName);

            var role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                logger.LogInformation("Role {RoleName} created successfully", roleName);
            }
            else
            {
                logger.LogWarning("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
