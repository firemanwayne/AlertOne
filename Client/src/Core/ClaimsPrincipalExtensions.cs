namespace System.Security.Claims;
/// <summary>
/// Extensions for retrieving various common properties of a user
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user identifier (the <c>sub</c> claim), or an empty string.
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("sub"))?.Value ?? string.Empty;

    /// <summary>
    /// Gets the email address (the <c>email</c> claim), or an empty string.
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("email"))?.Value ?? string.Empty;

    /// <summary>
    /// Gets the first name (the <c>firstname</c> claim), or an empty string.
    /// </summary>
    public static string GetFirstName(this ClaimsPrincipal p) => p.Claims
            .FirstOrDefault(a => a.Type.Equals("firstname"))?.Value ?? string.Empty;

    /// <summary>
    /// Gets the last name (the <c>lastname</c> claim), or an empty string.
    /// </summary>
    public static string GetLastName(this ClaimsPrincipal p) => p.Claims
           .FirstOrDefault(a => a.Type.Equals("lastname"))?.Value ?? string.Empty;
}
