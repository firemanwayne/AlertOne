using AlertOne.Api.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Azure.NotificationHubs.Services;

/// <summary>
/// Local development stand-in for <see cref="IPushNotificationService"/> that only logs. Used when no Notification Hubs connection string is configured.
/// </summary>
internal sealed class LoggingPushNotificationService(ILogger<LoggingPushNotificationService> logger) : IPushNotificationService
{
    /// <inheritdoc />
    public Task<string> RegisterDeviceAsync(string deviceToken, string platform, string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Register device skipped for user {UserId} on platform {Platform}", userId, platform);

        return Task.FromResult(Guid.NewGuid().ToString("N"));
    }

    /// <inheritdoc />
    public Task UnregisterDeviceAsync(string registrationId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Unregister device skipped for registration {RegistrationId}", registrationId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> SendToUserAsync(string userId, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Push to user {UserId} skipped: {Title}", userId, title);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> SendToTagsAsync(IEnumerable<string> tags, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Push to tags {Tags} skipped: {Title}", string.Join(",", tags), title);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> SendToAllAsync(string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Push to all skipped: {Title}", title);

        return Task.FromResult(true);
    }
}
