using AlertManagement.Infrastructure.Data;
using AlertOne.Api.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlertManagement.Client.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for AlertManagement integration tests.
/// Provides a configured service provider with in-memory database and mocked external services.
/// </summary>
public sealed class AlertManagementTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
    public AlertDbContext DbContext { get; }
    public Mock<IPushNotificationService> PushNotificationServiceMock { get; }

    public AlertManagementTestFixture()
    {
        // Build configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureNotificationHubsOptions:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=Test;SharedAccessKey=TestKey",
                ["AzureNotificationHubsOptions:HubName"] = "test-hub"
            })
            .Build();

        // Create mocks
        PushNotificationServiceMock = new Mock<IPushNotificationService>();

        // Setup default mock behaviors
        SetupDefaultMockBehaviors();

        // Build service provider
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));

        // Add in-memory database
        services.AddDbContext<AlertDbContext>(options =>
            options.UseInMemoryDatabase($"AlertManagement_Tests_{Guid.NewGuid()}"));

        // Add mocked services
        services.AddSingleton(PushNotificationServiceMock.Object);

        // Add configuration
        services.AddSingleton(Configuration);

        ServiceProvider = services.BuildServiceProvider();

        // Get DbContext and ensure database is created
        DbContext = ServiceProvider.GetRequiredService<AlertDbContext>();
        DbContext.Database.EnsureCreated();
    }

    private void SetupDefaultMockBehaviors()
    {
        // Default: SendToUserAsync returns success
        PushNotificationServiceMock
            .Setup(x => x.SendToUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Default: SendToAllAsync returns success
        PushNotificationServiceMock
            .Setup(x => x.SendToAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Default: SendToTagsAsync returns success
        PushNotificationServiceMock
            .Setup(x => x.SendToTagsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Default: RegisterDeviceAsync returns a registration ID
        PushNotificationServiceMock
            .Setup(x => x.RegisterDeviceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test-registration-id");
    }

    /// <summary>
    /// Resets the database to a clean state.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Gets a service from the service provider.
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();

        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
