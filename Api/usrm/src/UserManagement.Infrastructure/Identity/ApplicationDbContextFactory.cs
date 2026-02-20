using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Design-time factory for ApplicationDbContext for EF Core migrations
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder
            .UseAzureSql("Server=tcp:alert-one-sql-server-dev.database.windows.net,1433;Initial Catalog=alert-one-sql-db-dev;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication='Active Directory Default'");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
