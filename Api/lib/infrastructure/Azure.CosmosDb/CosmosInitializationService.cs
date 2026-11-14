using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azure.CosmosDb;

/// <summary>
/// Background service that creates the Cosmos DB database and containers for <typeparamref name="TContext"/> without blocking host startup.
/// </summary>
/// <typeparam name="TContext">The EF Core context mapped to Cosmos DB.</typeparam>
internal sealed class CosmosInitializationService<TContext>(IServiceProvider services, Func<IServiceProvider, CancellationToken, Task>? onInitialized, ILogger<CosmosInitializationService<TContext>> logger) : BackgroundService where TContext : DbContext
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await services
                .CreateCosmosIfNotExistsAsync<TContext>(stoppingToken)
                .ConfigureAwait(false);

            if (onInitialized is not null)
            {
                await onInitialized(services, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cosmos DB initialization failed for {Context}", typeof(TContext).Name);
        }
    }
}
