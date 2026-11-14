using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Services;

/// <summary>
/// Service for device/session management operations
/// </summary>
internal sealed class DeviceService(
    ApplicationDbContext context,
    ILogger<DeviceService> logger) : IDeviceService
{
    /// <summary>
    /// The maximum number of active devices a user can have; registering more fails with a conflict.
    /// </summary>
    private const int MaxDevicesPerUser = 20;

    /// <inheritdoc />
    public async Task<Result<RegisterDeviceResponse>> RegisterOrUpdateDeviceAsync(UserId userId, DeviceInfo deviceInfo, string? ipAddress, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Registering or updating device {DeviceId} for user {UserId}", deviceInfo.DeviceId, userId.Value);

        var existingDevice = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.UserId == userId.Value && d.DeviceId == deviceInfo.DeviceId, cancellationToken);

        if (existingDevice is not null)
        {
            // Update existing device
            existingDevice.DeviceName = deviceInfo.DeviceName;
            existingDevice.Platform = deviceInfo.Platform;
            existingDevice.AppVersion = deviceInfo.AppVersion;
            existingDevice.UserAgent = deviceInfo.UserAgent;
            existingDevice.PushNotificationToken = deviceInfo.PushNotificationToken;
            existingDevice.IpAddress = ipAddress;
            existingDevice.LastLoginAt = DateTime.UtcNow;
            existingDevice.LastActivityAt = DateTime.UtcNow;
            existingDevice.IsActive = true;
            existingDevice.RevokedAt = null;

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            existingDevice.RefreshToken = newRefreshToken;
            existingDevice.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Device {DeviceId} updated for user {UserId}", deviceInfo.DeviceId, userId.Value);

            var updatedDeviceResponse = MapToDeviceSessionResponse(existingDevice, existingDevice.Id);

            return Result.Success(new RegisterDeviceResponse(newRefreshToken, updatedDeviceResponse));
        }

        // Check device limit
        var activeDeviceCount = await context
            .Set<UserDevice>()
            .CountAsync(d => d.UserId == userId.Value && d.IsActive, cancellationToken);

        if (activeDeviceCount >= MaxDevicesPerUser)
        {
            logger.LogWarning("User {UserId} has reached maximum device limit", userId.Value);

            return Result.Failure<RegisterDeviceResponse>(
                Error.Conflict("Device.LimitReached", $"Maximum of {MaxDevicesPerUser} devices allowed. Please revoke an existing device first."));
        }

        // Create new device
        var refreshToken = GenerateRefreshToken();
        var now = DateTime.UtcNow;

        var device = new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            DeviceId = deviceInfo.DeviceId,
            DeviceName = deviceInfo.DeviceName,
            DeviceType = deviceInfo.DeviceType,
            Platform = deviceInfo.Platform,
            AppVersion = deviceInfo.AppVersion,
            UserAgent = deviceInfo.UserAgent,
            PushNotificationToken = deviceInfo.PushNotificationToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = now.AddDays(7),
            IsTrusted = false,
            IsActive = true,
            IpAddress = ipAddress,
            FirstLoginAt = now,
            LastLoginAt = now,
            LastActivityAt = now,
            CreatedAt = now
        };

        context
            .Set<UserDevice>()
            .Add(device);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Device {DeviceId} registered for user {UserId}", deviceInfo.DeviceId, userId.Value);

        var deviceResponse = MapToDeviceSessionResponse(device, device.Id);

        return Result.Success(new RegisterDeviceResponse(refreshToken, deviceResponse));
    }

    /// <inheritdoc />
    public async Task<Result<ValidateRefreshTokenResponse>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validating refresh token");

        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Refresh token not found");

            return Result.Failure<ValidateRefreshTokenResponse>(
                Error.Unauthorized("RefreshToken.Invalid", "Invalid refresh token"));
        }

        if (!device.IsActive)
        {
            logger.LogWarning("Device {DeviceId} is not active", device.Id);

            return Result.Failure<ValidateRefreshTokenResponse>(
                Error.Unauthorized("Device.Revoked", "Device has been revoked"));
        }

        if (device.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token expired for device {DeviceId}", device.Id);

            return Result.Failure<ValidateRefreshTokenResponse>(
                Error.Unauthorized("RefreshToken.Expired", "Refresh token has expired"));
        }

        logger.LogInformation("Refresh token validated for device {DeviceId}", device.Id);

        return Result.Success(new ValidateRefreshTokenResponse(UserId.From(device.UserId), device.Id));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DeviceSessionResponse>>> GetActiveDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting active devices for user {UserId}", userId.Value);

        var devices = await context
            .Set<UserDevice>()
            .Where(d => d.UserId == userId.Value && d.IsActive)
            .OrderByDescending(d => d.LastActivityAt ?? d.LastLoginAt)
            .ToListAsync(cancellationToken);

        var deviceResponses = devices
            .Select(d => MapToDeviceSessionResponse(d, null))
            .ToList();

        return Result.Success<IEnumerable<DeviceSessionResponse>>(deviceResponses);
    }

    /// <inheritdoc />
    public async Task<Result<DeviceSessionResponse>> GetDeviceByIdAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting device {DeviceId} for user {UserId}", deviceId, userId.Value);

        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId.Value, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Device {DeviceId} not found for user {UserId}", deviceId, userId.Value);

            return Result.Failure<DeviceSessionResponse>(
                Error.NotFound("Device.NotFound", $"Device with ID {deviceId} not found"));
        }

        var deviceResponse = MapToDeviceSessionResponse(device, deviceId);

        return Result.Success(deviceResponse);
    }

    /// <inheritdoc />
    public async Task<Result> RevokeDeviceAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Revoking device {DeviceId} for user {UserId}", deviceId, userId.Value);

        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId.Value, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Device {DeviceId} not found for user {UserId}", deviceId, userId.Value);

            return Result.Failure(
                Error.NotFound("Device.NotFound", $"Device with ID {deviceId} not found"));
        }

        device.IsActive = false;
        device.RevokedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Device {DeviceId} revoked for user {UserId}", deviceId, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RevokeAllDevicesExceptAsync(UserId userId, Guid exceptDeviceId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Revoking all devices except {DeviceId} for user {UserId}", exceptDeviceId, userId.Value);

        var devices = await context
            .Set<UserDevice>()
            .Where(d => d.UserId == userId.Value && d.Id != exceptDeviceId && d.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var device in devices)
        {
            device.IsActive = false;
            device.RevokedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked {Count} devices for user {UserId}", devices.Count, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RevokeAllDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Revoking all devices for user {UserId}", userId.Value);

        var devices = await context
            .Set<UserDevice>()
            .Where(d => d.UserId == userId.Value && d.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var device in devices)
        {
            device.IsActive = false;
            device.RevokedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked {Count} devices for user {UserId}", devices.Count, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> UpdateLastActivityAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

        if (device is null)
        {
            return Result.Failure(
                Error.NotFound("Device.NotFound", $"Device with ID {deviceId} not found"));
        }

        device.LastActivityAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> TrustDeviceAsync(UserId userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Marking device {DeviceId} as trusted for user {UserId}", deviceId, userId.Value);

        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId.Value, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Device {DeviceId} not found for user {UserId}", deviceId, userId.Value);

            return Result.Failure(
                Error.NotFound("Device.NotFound", $"Device with ID {deviceId} not found"));
        }

        device.IsTrusted = true;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Device {DeviceId} marked as trusted for user {UserId}", deviceId, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<DeviceSessionResponse>> UpdateDeviceNameAsync(UserId userId, Guid deviceId, string newName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating device name for device {DeviceId}", deviceId);

        var device = await context
            .Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId.Value, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Device {DeviceId} not found for user {UserId}", deviceId, userId.Value);

            return Result.Failure<DeviceSessionResponse>(
                Error.NotFound("Device.NotFound", $"Device with ID {deviceId} not found"));
        }

        device.DeviceName = newName;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Device name updated for device {DeviceId}", deviceId);

        var deviceResponse = MapToDeviceSessionResponse(device, deviceId);

        return Result.Success(deviceResponse);
    }

    /// <summary>
    /// Generates a cryptographically random refresh token.
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DeviceNotificationInfo>>> GetDevicesForNotificationsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting devices for notifications for user {UserId}", userId.Value);

        var devices = await context
            .Set<UserDevice>()
            .Where(d => d.UserId == userId.Value && d.IsActive && d.PushNotificationToken != null)
            .ToListAsync(cancellationToken);

        var deviceNotifications = devices
            .Select(d => new DeviceNotificationInfo(
                d.Id,
                d.DeviceType,
                d.Platform,
                d.PushNotificationToken,
                d.IsActive))
            .ToList();

        return Result.Success<IEnumerable<DeviceNotificationInfo>>(deviceNotifications);
    }

    /// <summary>
    /// Maps a device to its session response, flagging the current device.
    /// </summary>
    private static DeviceSessionResponse MapToDeviceSessionResponse(UserDevice device, Guid? currentDeviceId)
    {
        return new DeviceSessionResponse(
            device.Id,
            device.DeviceName,
            device.DeviceType,
            device.Platform,
            device.AppVersion,
            device.IsTrusted,
            currentDeviceId.HasValue && currentDeviceId.Value == device.Id,
            device.IpAddress,
            device.Location,
            device.FirstLoginAt,
            device.LastLoginAt,
            device.LastActivityAt,
            device.PushNotificationToken);
    }
}
