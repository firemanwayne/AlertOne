namespace Azure.NotificationHubs.Options;

/// <summary>
/// Configuration options for Azure Notification Hubs.
/// </summary>
public sealed class AzureNotificationHubsOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AzureNotificationHubs";

    /// <summary>
    /// The connection string for Azure Notification Hub.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Notification Hub.
    /// </summary>
    public string HubName { get; set; } = string.Empty;
}
