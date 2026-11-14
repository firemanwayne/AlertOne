using Microsoft.EntityFrameworkCore;

using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Tests.Helpers;

/// <summary>
/// Creates real <see cref="ApplicationDbContext"/> instances backed by the EF Core in-memory provider.
/// The context is sealed, so it cannot be mocked, and the services run real queries against it.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a context on a new, empty in-memory database that is isolated from every other test.
    /// </summary>
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
