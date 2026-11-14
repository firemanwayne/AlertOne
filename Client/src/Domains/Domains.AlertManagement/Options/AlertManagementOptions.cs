namespace Domains.AlertManagement.Options;

/// <summary>
/// Options for the Alert Management API client.
/// </summary>
internal class AlertManagementOptions : ApiOptionsBase
{
    /// <summary>
    /// The name of the HTTP client used for the Alert Management API.
    /// </summary>
    public const string HttpClientName = "Alert-Management- Client";

    /// <inheritdoc />
    public override string GetHttpClientName()
    {
        return HttpClientName;
    }
}
