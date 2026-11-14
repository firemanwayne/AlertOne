using UserManagement.Core.Enums;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Represents a device/session for a user
/// </summary>
public sealed class UserDevice
{
    /// <summary>
    /// Gets or sets the device record identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the owning user.
    /// </summary>
    public Guid UserId { get; set; }

    // Device Identification
    /// <summary>
    /// Gets or sets the client-generated device identifier.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the display name of the device.
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the kind of device (web, mobile, desktop).
    /// </summary>
    public DeviceType DeviceType { get; set; }
    /// <summary>
    /// Gets or sets the platform (for example Windows or Android).
    /// </summary>
    public string Platform { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the app version reported by the device.
    /// </summary>
    public string? AppVersion { get; set; }
    /// <summary>
    /// Gets or sets the user agent reported by the device.
    /// </summary>
    public string? UserAgent { get; set; }

    // Security & Session
    /// <summary>
    /// Gets or sets the refresh token issued to the device.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets when the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiryTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user trusts the device.
    /// </summary>
    public bool IsTrusted { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the device session is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Push Notifications
    /// <summary>
    /// Gets or sets the push notification token of the device.
    /// </summary>
    public string? PushNotificationToken { get; set; }

    // Tracking
    /// <summary>
    /// Gets or sets the IP address seen at the last login.
    /// </summary>
    public string? IpAddress { get; set; }
    /// <summary>
    /// Gets or sets the approximate location of the device.
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// Gets or sets the UTC time of the first login from the device.
    /// </summary>
    public DateTime FirstLoginAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time of the most recent login from the device.
    /// </summary>
    public DateTime LastLoginAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time of the most recent activity.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    // Timestamps
    /// <summary>
    /// Gets or sets the UTC time the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time the device was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    // Navigation property
    /// <summary>
    /// Gets or sets the owning user (navigation property).
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
