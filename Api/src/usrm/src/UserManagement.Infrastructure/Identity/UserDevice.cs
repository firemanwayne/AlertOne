using UserManagement.Core.Enums;

namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Represents a device/session for a user
/// </summary>
public sealed class UserDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Device Identification
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
    public string? UserAgent { get; set; }

    // Security & Session
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool IsTrusted { get; set; }
    public bool IsActive { get; set; } = true;

    // Push Notifications
    public string? PushNotificationToken { get; set; }

    // Tracking
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime FirstLoginAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
