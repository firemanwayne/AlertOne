using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlertManagement.Infrastructure.Data;

/// <summary>
/// Design-time factory for AlertDbContext for EF Core migrations
/// </summary>
public sealed class AlertDbContextFactory : IDesignTimeDbContextFactory<AlertDbContext>
{
    public AlertDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetValue<string>("AzureSqlServer:ConnectionString")
            ?? configuration.GetConnectionString("AlertDb");

        var optionsBuilder = new DbContextOptionsBuilder<AlertDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new AlertDbContext(optionsBuilder.Options);
    }
}
