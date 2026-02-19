using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlertManagement.Infrastructure.Data;

/// <summary>
/// Design-time factory for AlertDbContext for EF Core migrations
/// </summary>
public sealed class AlertDbContextFactory : IDesignTimeDbContextFactory<AlertDbContext>
{
    public AlertDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AlertDbContext>();

        optionsBuilder
            .UseAzureSql("Server=tcp:alert-one-sql-server-dev.database.windows.net,1433;Initial Catalog=alert-one-sql-db-dev;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication='Active Directory Default'");

        return new AlertDbContext(optionsBuilder.Options);
    }
}
