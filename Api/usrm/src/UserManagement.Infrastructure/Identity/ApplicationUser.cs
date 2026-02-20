using Microsoft.AspNetCore.Identity;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Represents an application user with Identity framework
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
