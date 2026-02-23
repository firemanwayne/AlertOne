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
    public async Task<Result<UserSettingsResponse>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting settings for user: {UserId}", userId.Value);

        var settings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (settings is null)
        {
            logger.LogWarning("Settings not found for user: {UserId}", userId.Value);

            return Result.Failure<UserSettingsResponse>(
                Error.NotFound("UserSettings.NotFound", $"Settings not found for user {userId.Value}"));
        }

        return Result.Success(MapToResponse(settings));
    }

    public async Task<Result<UserSettingsResponse>> UpdateAsync(UserId userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating settings for user: {UserId}", userId.Value);

        var settings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

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

    private static UserSettingsResponse MapToResponse(UserSettings settings) =>
        new(settings.Id, settings.UserId, settings.ReceiveEmailNotifications, settings.ReceivePushNotifications, settings.PreferredTheme, settings.CreatedDate, settings.UpdatedDate);
}
