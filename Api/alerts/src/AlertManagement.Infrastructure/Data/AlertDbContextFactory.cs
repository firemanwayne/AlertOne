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

        // Use SQLite for migrations (can be changed as needed)
        optionsBuilder.UseSqlite("Data Source=alertmanagement.db");

        return new AlertDbContext(optionsBuilder.Options);
    }
}
