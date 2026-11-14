namespace Azure.Communications.Options;

/// <summary>
/// Configuration options for Azure Communication Services.
/// </summary>
public sealed class AzureCommunicationsOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AzureCommunications";

    /// <summary>
    /// The connection string for Azure Communication Services.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The sender email address for outgoing emails.
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>
    /// The base URL for the web application (used in email links).
    /// </summary>
    public string WebAppBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The deep link scheme for the mobile app (used in email links).
    /// </summary>
    public string MobileAppScheme { get; set; } = "alertone";
}
