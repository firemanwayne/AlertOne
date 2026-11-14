using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Services;

/// <summary>
/// Service for user settings operations
/// </summary>
internal sealed class UserSettingsService(ILogger<UserSettingsService> logger, ApplicationDbContext context) : IUserSettingsService
{
    /// <inheritdoc />
    public async Task<Result<UserSettingsResponse>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting settings for user: {UserId}", userId.Value);

        var settings = await FindOrProvisionAsync(userId, cancellationToken);

        if (settings is null)
        {
            logger.LogWarning("Settings not found for user: {UserId}", userId.Value);

            return Result.Failure<UserSettingsResponse>(
                Error.NotFound("UserSettings.NotFound", $"Settings not found for user {userId.Value}"));
        }

        return Result.Success(MapToResponse(settings));
    }

    /// <inheritdoc />
    public async Task<Result<UserSettingsResponse>> UpdateAsync(UserId userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating settings for user: {UserId}", userId.Value);

        var settings = await FindOrProvisionAsync(userId, cancellationToken);

        if (settings is null)
        {
            logger.LogWarning("Settings not found for user: {UserId}", userId.Value);

            return Result.Failure<UserSettingsResponse>(
                Error.NotFound("UserSettings.NotFound", $"Settings not found for user {userId.Value}"));
        }

        settings.ReceiveEmailNotifications = request.ReceiveEmailNotifications;
        settings.ReceivePushNotifications = request.ReceivePushNotifications;
        settings.PreferredTheme = request.PreferredTheme;
        settings.UpdatedDate = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Settings updated for user: {UserId}", userId.Value);

        return Result.Success(MapToResponse(settings));
    }

    /// <inheritdoc />
    public async Task<Result<UserSettingsResponse>> CreateDefaultAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating default settings for user: {UserId}", userId.Value);

        var existingSettings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (existingSettings is not null)
        {
            logger.LogWarning("Settings already exist for user: {UserId}", userId.Value);

            return Result.Success(MapToResponse(existingSettings));
        }

        var now = DateTime.UtcNow;

        var settings = new UserSettings
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId.Value,
            ReceiveEmailNotifications = true,
            ReceivePushNotifications = true,
            PreferredTheme = null,
            CreatedDate = now,
            UpdatedDate = now
        };

        context.UserSettings.Add(settings);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Default settings created for user: {UserId}", userId.Value);

        return Result.Success(MapToResponse(settings));
    }

    /// <summary>
    /// Finds a user's settings. When the user exists but has none (for example an account created before default settings were provisioned), creates the defaults.
    /// </summary>
    private async Task<UserSettings?> FindOrProvisionAsync(UserId userId, CancellationToken cancellationToken)
    {
        var settings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        // Note: AnyAsync generates malformed Cosmos SQL, so read the user instead
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return null;
        }

        logger.LogInformation("Provisioning default settings for existing user: {UserId}", userId.Value);

        await CreateDefaultAsync(userId, cancellationToken);

        return await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);
    }

    /// <summary>
    /// Maps a settings entity to its response.
    /// </summary>
    private static UserSettingsResponse MapToResponse(UserSettings settings) =>
        new(settings.Id, settings.UserId, settings.ReceiveEmailNotifications, settings.ReceivePushNotifications, settings.PreferredTheme, settings.CreatedDate, settings.UpdatedDate);
}
