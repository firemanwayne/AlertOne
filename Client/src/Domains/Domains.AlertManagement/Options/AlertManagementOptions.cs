namespace Domains.AlertManagement.Options;

internal class AlertManagementOptions : ApiOptionsBase
{
    public const string HttpClientName = "Alert-Management- Client";

    public override string GetHttpClientName()
    {
        return HttpClientName;
    }
}
