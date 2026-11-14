using AlertOne.Api.Core.Abstractions;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Represents user-specific settings and preferences
/// </summary>
public sealed class UserSettings : DataModelBase
{
    /// <summary>
    /// Gets or sets the identifier of the user the settings belong to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether the user wants to receive email notifications
    /// </summary>
    public bool ReceiveEmailNotifications { get; set; } = true;

    /// <summary>
    /// Whether the user wants to receive push notifications
    /// </summary>
    public bool ReceivePushNotifications { get; set; } = true;

    /// <summary>
    /// User's preferred theme (placeholder for future implementation)
    /// </summary>
    public string? PreferredTheme { get; set; }

    // Navigation property
    /// <summary>
    /// Gets or sets the owning user (navigation property).
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
