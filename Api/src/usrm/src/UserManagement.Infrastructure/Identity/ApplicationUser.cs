using Microsoft.AspNetCore.Identity;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Represents an application user with Identity framework
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the UTC time the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time the user was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
}
