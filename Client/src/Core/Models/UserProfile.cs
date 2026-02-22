namespace AlertOne.Core.Models;

/// <summary>
/// Represents a user profile
/// </summary>
public sealed class UserProfile
{
    /// <summary>
    /// Gets or sets the user's unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's roles
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets the user's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets a value indicating whether the user is an Admin
    /// </summary>
    public bool IsAdmin => HasRole("Admin");

    /// <summary>
    /// Gets a value indicating whether the user is a Manager
    /// </summary>
    public bool IsManager => HasRole("Manager");

    /// <summary>
    /// Gets a value indicating whether the user has management privileges (Admin or Manager)
    /// </summary>
    public bool CanManageUsers => IsAdmin || IsManager;

    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role</returns>
    public bool HasRole(string role) => Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
}
