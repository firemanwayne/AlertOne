using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Design-time factory for ApplicationDbContext for EF Core migrations
/// </summary>
public sealed class ApplicationDbContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = configuration.GetValue<string>("AzureSqlServer:ConnectionString");

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder
            .UseAzureSql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
