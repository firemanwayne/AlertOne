using AlertOne.Core.Abstractions;

namespace Domains.UserManagement.Options;

/// <summary>
/// Configuration options for UserManagement API
/// </summary>
internal class UserManagementOptions : ApiOptionsBase
{
    /// <summary>
    /// The name of the HTTP client used for the User Management API.
    /// </summary>
    public const string HttpClientName = "User-Management-Client";

    /// <inheritdoc />
    public override string GetHttpClientName()
    {
        return HttpClientName;
    }
}
