using Azure.AppConfig;
using Azure.Communications;
using Azure.CosmosDb;
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

// Create the Cosmos DB database/containers if missing, then seed default roles (runs in the background, retries until Cosmos DB is ready)
builder
    .Services
    .AddCosmosInitialization<ApplicationDbContext>(SeedRolesAsync);

builder
    .Services
    .AddAzureCommunications(builder.Configuration);

builder
    .UseMiddleware<ExceptionHandlingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>();

var host = builder.Build();

await host
    .RunAsync();

static async Task SeedRolesAsync(IServiceProvider rootServices, CancellationToken cancellationToken)
{
    using var scope = rootServices.CreateScope();

    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

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
