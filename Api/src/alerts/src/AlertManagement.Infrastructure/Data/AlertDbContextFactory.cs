using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlertManagement.Infrastructure.Data;

/// <summary>
/// Design-time factory for AlertDbContext for EF Core migrations
/// </summary>
public sealed class AlertDbContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<AlertDbContext>
{
    public AlertDbContext CreateDbContext(string[] args)
    {
        var connectionString = configuration.GetValue<string>("AzureSqlServer:ConnectionString");

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<AlertDbContext>();

        optionsBuilder
            .UseAzureSql(connectionString);

        return new AlertDbContext(optionsBuilder.Options);
    }
}
