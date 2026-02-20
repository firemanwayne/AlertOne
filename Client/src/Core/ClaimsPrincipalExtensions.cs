namespace System.Security.Claims;
/// <summary>
/// Extensions for retrieving various common properties of a user
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("sub"))?.Value ?? string.Empty;

    public static string GetEmail(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("email"))?.Value ?? string.Empty;

    public static string GetFirstName(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("firstname"))?.Value ?? string.Empty;

    public static string GetLastName(this ClaimsPrincipal p) => p.Claims
           .FirstOrDefault(a => a.Type.Equals("lastname"))?.Value ?? string.Empty;
}
