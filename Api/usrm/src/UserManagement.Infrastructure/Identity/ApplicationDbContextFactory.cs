using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Design-time factory for ApplicationDbContext for EF Core migrations
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetValue<string>("AzureSqlServer:ConnectionString")
            ?? configuration.GetValue<string>("Values:AzureSqlServer:ConnectionString")
            ?? Environment.GetEnvironmentVariable("AzureSqlServer__ConnectionString");

        // For design-time migration generation, use a placeholder if no connection string is available
        // Migrations don't require a real connection - they just analyze the model
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=.;Database=UserManagement_Design;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
