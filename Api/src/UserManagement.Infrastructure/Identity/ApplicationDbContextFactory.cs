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

        // Use SQLite for migrations (can be changed as needed)
        optionsBuilder.UseSqlite("Data Source=usermanagement.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
