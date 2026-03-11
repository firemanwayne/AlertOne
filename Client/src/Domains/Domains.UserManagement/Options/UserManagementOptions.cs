using AlertOne.Core.Abstractions;

namespace Domains.UserManagement.Options;

/// <summary>
/// Configuration options for UserManagement API
/// </summary>
internal class UserManagementOptions : ApiOptionsBase
{
    public const string HttpClientName = "User-Management-Client";

    public override string GetHttpClientName()
    {
        return HttpClientName;
    }
}
