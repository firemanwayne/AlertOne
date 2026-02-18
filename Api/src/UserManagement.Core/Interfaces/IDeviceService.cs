using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for device/session management operations
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Registers or updates a device for a user and generates a refresh token
    /// </summary>
    Task<Result<RegisterDeviceResponse>> RegisterOrUpdateDeviceAsync(UserId userId, DeviceInfo deviceInfo, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token and returns the associated device
    /// </summary>
    Task<Result<ValidateRefreshTokenResponse>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active devices for a user
    /// </summary>
    Task<Result<IEnumerable<DeviceSessionResponse>>> GetActiveDevicesAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific device by ID
    /// </summary>
    Task<Result<DeviceSessionResponse>> GetDeviceByIdAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific device
    /// </summary>
    Task<Result> RevokeDeviceAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all devices except the specified one
    /// </summary>
    Task<Result> RevokeAllDevicesExceptAsync(UserId userId, Guid exceptDeviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all devices for a user
    /// </summary>
    Task<Result> RevokeAllDevicesAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last activity time for a device
    /// </summary>
    Task<Result> UpdateLastActivityAsync(Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a device as trusted
    /// </summary>
    Task<Result> TrustDeviceAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates device name
    /// </summary>
    Task<Result<DeviceSessionResponse>> UpdateDeviceNameAsync(UserId userId, Guid deviceId, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets devices for notification purposes
    /// </summary>
    Task<Result<IEnumerable<DeviceNotificationInfo>>> GetDevicesForNotificationsAsync(UserId userId, CancellationToken cancellationToken = default);
}
