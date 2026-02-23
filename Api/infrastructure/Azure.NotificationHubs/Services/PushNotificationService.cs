using AlertOne.Api.Core.Interfaces;
using Azure.NotificationHubs.Options;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.NotificationHubs.Services;

/// <summary>
/// Implementation of push notification service using Azure Notification Hubs.
/// </summary>
internal sealed class PushNotificationService : IPushNotificationService
{
    private readonly NotificationHubClient _hubClient;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(IOptions<AzureNotificationHubs> options, ILogger<PushNotificationService> logger)
    {
        _logger = logger;

        var connectionString = options.Value.ConnectionString;
        var hubName = options.Value.HubName;

        _logger.LogInformation("Initializing Notification Hub. HubName: {HubName}, ConnectionString starts with: {ConnectionStringPrefix}",
            hubName,
            connectionString.Length > 50 ? connectionString[..50] + "..." : connectionString);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("AzureNotificationHubs:ConnectionString is not configured");
        }

        if (string.IsNullOrEmpty(hubName))
        {
            throw new InvalidOperationException("AzureNotificationHubs:HubName is not configured");
        }

        _hubClient = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
    }

    private ILogger<PushNotificationService> logger => _logger;

    public async Task<string> RegisterDeviceAsync(string deviceToken, string platform, string userId, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Registering device for user {UserId} on platform {Platform}", userId, platform);

        var tagList = new List<string> { $"userId:{userId}" };

        if (tags is not null)
        {
            tagList.AddRange(tags);
        }

        try
        {
            var registration = platform.ToLowerInvariant() switch
            {
                "fcm" or "android" => await CreateFcmRegistrationAsync(deviceToken, tagList, cancellationToken),
                "apns" or "ios" => await CreateApnsRegistrationAsync(deviceToken, tagList, cancellationToken),
                "wns" or "windows" => await CreateWnsRegistrationAsync(deviceToken, tagList, cancellationToken),
                _ => throw new ArgumentException($"Unsupported platform: {platform}", nameof(platform))
            };

            logger.LogInformation("Device registered successfully with ID {RegistrationId}", registration.RegistrationId);

            return registration.RegistrationId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register device for user {UserId}", userId);
            throw;
        }
    }

    public async Task UnregisterDeviceAsync(string registrationId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Unregistering device with ID {RegistrationId}", registrationId);

        try
        {
            await _hubClient.DeleteRegistrationAsync(registrationId, cancellationToken);

            logger.LogInformation("Device unregistered successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unregister device {RegistrationId}", registrationId);
            throw;
        }
    }

    public async Task<bool> SendToUserAsync(string userId, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending push notification to user {UserId}", userId);

        var tagExpression = $"userId:{userId}";

        return await SendNotificationAsync(tagExpression, title, body, data, cancellationToken);
    }

    public async Task<bool> SendToTagsAsync(IEnumerable<string> tags, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var tagList = tags.ToList();

        logger.LogInformation("Sending push notification to tags: {Tags}", string.Join(", ", tagList));

        var tagExpression = string.Join(" || ", tagList);

        return await SendNotificationAsync(tagExpression, title, body, data, cancellationToken);
    }

    public async Task<bool> SendToAllAsync(string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending push notification to all devices");

        var success = false;

        // Try FCM (Android)
        try
        {
            var fcmPayload = BuildFcmPayload(title, body, data);
            var fcmOutcome = await _hubClient.SendFcmV1NativeNotificationAsync(fcmPayload, cancellationToken);
            logger.LogInformation("FCM notification sent. State: {State}", fcmOutcome.State);
            success = fcmOutcome.State is NotificationOutcomeState.Enqueued or NotificationOutcomeState.Completed || success;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send FCM notification (Android). This may be because FCM is not configured in the Notification Hub.");
        }

        // Try APNS (iOS)
        try
        {
            var apnsPayload = BuildApnsPayload(title, body, data);
            var apnsOutcome = await _hubClient.SendAppleNativeNotificationAsync(apnsPayload, cancellationToken);
            logger.LogInformation("APNS notification sent. State: {State}", apnsOutcome.State);
            success = apnsOutcome.State is NotificationOutcomeState.Enqueued or NotificationOutcomeState.Completed || success;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send APNS notification (iOS). This may be because APNS is not configured in the Notification Hub.");
        }

        if (!success)
        {
            logger.LogWarning("No notifications were sent successfully. Ensure FCM and/or APNS credentials are configured in Azure Notification Hub.");
        }

        return success;
    }

    private async Task<bool> SendNotificationAsync(string tagExpression, string title, string body, IDictionary<string, string>? data, CancellationToken cancellationToken)
    {
        var success = false;

        // Try FCM (Android)
        try
        {
            var fcmPayload = BuildFcmPayload(title, body, data);
            var fcmOutcome = await _hubClient.SendFcmV1NativeNotificationAsync(fcmPayload, tagExpression, cancellationToken);
            logger.LogInformation("FCM notification sent to tags '{Tags}'. State: {State}", tagExpression, fcmOutcome.State);
            success = fcmOutcome.State is NotificationOutcomeState.Enqueued or NotificationOutcomeState.Completed || success;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send FCM notification to tags '{Tags}'", tagExpression);
        }

        // Try APNS (iOS)
        try
        {
            var apnsPayload = BuildApnsPayload(title, body, data);
            var apnsOutcome = await _hubClient.SendAppleNativeNotificationAsync(apnsPayload, tagExpression, cancellationToken);
            logger.LogInformation("APNS notification sent to tags '{Tags}'. State: {State}", tagExpression, apnsOutcome.State);
            success = apnsOutcome.State is NotificationOutcomeState.Enqueued or NotificationOutcomeState.Completed || success;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send APNS notification to tags '{Tags}'", tagExpression);
        }

        return success;
    }

    private async Task<RegistrationDescription> CreateFcmRegistrationAsync(string deviceToken, List<string> tags, CancellationToken cancellationToken)
    {
        // Get a new registration ID from the hub
        var registrationId = await _hubClient.CreateRegistrationIdAsync(cancellationToken);

        var registration = new FcmV1RegistrationDescription(deviceToken)
        {
            RegistrationId = registrationId,
            Tags = new HashSet<string>(tags)
        };

        return await _hubClient.CreateOrUpdateRegistrationAsync(registration, cancellationToken);
    }

    private async Task<RegistrationDescription> CreateApnsRegistrationAsync(string deviceToken, List<string> tags, CancellationToken cancellationToken)
    {
        // Get a new registration ID from the hub
        var registrationId = await _hubClient.CreateRegistrationIdAsync(cancellationToken);

        var registration = new AppleRegistrationDescription(deviceToken)
        {
            RegistrationId = registrationId,
            Tags = new HashSet<string>(tags)
        };

        return await _hubClient.CreateOrUpdateRegistrationAsync(registration, cancellationToken);
    }

    private async Task<RegistrationDescription> CreateWnsRegistrationAsync(string channelUri, List<string> tags, CancellationToken cancellationToken)
    {
        // Get a new registration ID from the hub
        var registrationId = await _hubClient.CreateRegistrationIdAsync(cancellationToken);

        var registration = new WindowsRegistrationDescription(channelUri)
        {
            RegistrationId = registrationId,
            Tags = new HashSet<string>(tags)
        };

        return await _hubClient.CreateOrUpdateRegistrationAsync(registration, cancellationToken);
    }

    private static string BuildFcmPayload(string title, string body, IDictionary<string, string>? data)
    {
        var dataJson = data is not null && data.Count > 0
            ? $", \"data\": {System.Text.Json.JsonSerializer.Serialize(data)}"
            : "";

        return $$"""
            {
                "message": {
                    "notification": {
                        "title": "{{EscapeJson(title)}}",
                        "body": "{{EscapeJson(body)}}"
                    }{{dataJson}}
                }
            }
            """;
    }

    private static string BuildApnsPayload(string title, string body, IDictionary<string, string>? data)
    {
        var dataJson = data is not null && data.Count > 0
            ? $", {string.Join(", ", data.Select(kvp => $"\"{EscapeJson(kvp.Key)}\": \"{EscapeJson(kvp.Value)}\""))}"
            : "";

        return $$"""
            {
                "aps": {
                    "alert": {
                        "title": "{{EscapeJson(title)}}",
                        "body": "{{EscapeJson(body)}}"
                    },
                    "sound": "default"
                }{{dataJson}}
            }
            """;
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
