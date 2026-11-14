using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.CosmosDb;

/// <summary>
/// Extension methods for creating a Cosmos DB database and its containers when they do not exist yet.
/// </summary>
public static class CosmosInitializationExtensions
{
    /// <summary>
    /// Maximum number of attempts before giving up (the Cosmos emulator can take a couple of minutes to accept requests on first start).
    /// </summary>
    private const int MaxAttempts = 30;

    /// <summary>
    /// Delay between attempts.
    /// </summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Registers a background service that creates the Cosmos DB database and containers for <typeparamref name="TContext"/> if they do not exist, without blocking host startup.
    /// </summary>
    /// <typeparam name="TContext">The EF Core context mapped to Cosmos DB.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="onInitialized">Optional callback run after the database exists (for example, seeding data).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCosmosInitialization<TContext>(this IServiceCollection services, Func<IServiceProvider, CancellationToken, Task>? onInitialized = null) where TContext : DbContext
    {
        services.AddHostedService(sp => new CosmosInitializationService<TContext>(sp, onInitialized, sp.GetRequiredService<ILogger<CosmosInitializationService<TContext>>>()));

        return services;
    }

    /// <summary>
    /// Creates the database and every container the context maps to if they do not already exist, retrying while Cosmos DB is not reachable yet.
    /// </summary>
    /// <typeparam name="TContext">The EF Core context mapped to Cosmos DB.</typeparam>
    /// <param name="services">The root service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the database was created by this call; false if it already existed.</returns>
    public static async Task<bool> CreateCosmosIfNotExistsAsync<TContext>(this IServiceProvider services, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        using var scope = services.CreateScope();

        var scopedServices = scope.ServiceProvider;

        var logger = scopedServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(CosmosInitializationExtensions).FullName ?? nameof(CosmosInitializationExtensions));

        var context = scopedServices.GetRequiredService<TContext>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                logger.LogInformation("Ensuring Cosmos DB database and containers exist for {Context} (attempt {Attempt}/{MaxAttempts})", typeof(TContext).Name, attempt, MaxAttempts);

                var created = await context
                    .Database
                    .EnsureCreatedAsync(cancellationToken)
                    .ConfigureAwait(false);

                logger.LogInformation("Cosmos DB ready for {Context}. Newly created: {Created}", typeof(TContext).Name, created);

                return created;
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < MaxAttempts)
            {
                logger.LogWarning(ex, "Cosmos DB is not ready for {Context} yet (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}s", typeof(TContext).Name, attempt, MaxAttempts, RetryDelay.TotalSeconds);

                await Task
                    .Delay(RetryDelay, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
