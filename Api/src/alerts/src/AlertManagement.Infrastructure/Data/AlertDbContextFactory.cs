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
            ?? configuration.GetConnectionString("AlertDb")
            ?? "Data Source=alert-one-sql-server-dev.database.windows.net;Initial Catalog=alert-one-sql-db-dev;User ID=wdavis2002@gmail.com;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Authentication=ActiveDirectoryInteractive;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

        var optionsBuilder = new DbContextOptionsBuilder<AlertDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new AlertDbContext(optionsBuilder.Options);
    }
}
