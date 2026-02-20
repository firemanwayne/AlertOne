using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;
using AlertManagement.Core.Entities;
using AlertManagement.Core.Interfaces;
using AlertManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlertManagement.Infrastructure.Services;

/// <summary>
/// Service for alert status level management operations
/// </summary>
internal sealed class AlertStatusLevelService(AlertDbContext context, ILogger<AlertStatusLevelService> logger) : IAlertStatusLevelService
{
    public async Task<Result<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all alert status levels");

        var statusLevels = await context.AlertStatusLevels
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var responses = statusLevels.Select(MapToResponse);

        return Result.Success(responses);
    }

    public async Task<Result<AlertStatusLevelResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting alert status level {AlertStatusLevelId}", id);

        var statusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (statusLevel is null)
        {
            logger.LogWarning("Alert status level not found: {AlertStatusLevelId}", id);
            return Result.Failure<AlertStatusLevelResponse>(
                Error.NotFound("AlertStatusLevel.NotFound", $"Alert status level with ID {id} not found"));
        }

        return Result.Success(MapToResponse(statusLevel));
    }

    public async Task<Result<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating alert status level: {AlertStatusLevelName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertStatusLevelResponse>(
                Error.Validation("AlertStatusLevel.NameRequired", "Alert status level name is required"));
        }

        var existingLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Name == request.Name, cancellationToken);

        if (existingLevel is not null)
        {
            logger.LogWarning("Alert status level already exists: {AlertStatusLevelName}", request.Name);
            return Result.Failure<AlertStatusLevelResponse>(
                Error.Conflict("AlertStatusLevel.AlreadyExists", $"Alert status level '{request.Name}' already exists"));
        }

        // If this is being set as default, clear any existing default
        if (request.IsDefault)
        {
            var existingDefault = await context.AlertStatusLevels
                .FirstOrDefaultAsync(s => s.IsDefault, cancellationToken);

            if (existingDefault is not null)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedDate = DateTime.UtcNow;
            }
        }

        // If this is being set as canceled, clear any existing canceled
        if (request.IsCanceled)
        {
            var existingCanceled = await context.AlertStatusLevels
                .FirstOrDefaultAsync(s => s.IsCanceled, cancellationToken);

            if (existingCanceled is not null)
            {
                existingCanceled.IsCanceled = false;
                existingCanceled.UpdatedDate = DateTime.UtcNow;
            }
        }

        var statusLevel = new AlertStatusLevel
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            SendNotifications = request.SendNotifications,
            IsDefault = request.IsDefault,
            IsCanceled = request.IsCanceled,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        context.AlertStatusLevels.Add(statusLevel);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert status level {AlertStatusLevelId} created successfully", statusLevel.Id);

        return Result.Success(MapToResponse(statusLevel));
    }

    public async Task<Result<AlertStatusLevelResponse>> UpdateAsync(string id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating alert status level {AlertStatusLevelId}", id);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertStatusLevelResponse>(
                Error.Validation("AlertStatusLevel.NameRequired", "Alert status level name is required"));
        }

        var statusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (statusLevel is null)
        {
            logger.LogWarning("Alert status level not found: {AlertStatusLevelId}", id);
            return Result.Failure<AlertStatusLevelResponse>(
                Error.NotFound("AlertStatusLevel.NotFound", $"Alert status level with ID {id} not found"));
        }

        var existingLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Name == request.Name && s.Id != id, cancellationToken);

        if (existingLevel is not null)
        {
            logger.LogWarning("Alert status level name already taken: {AlertStatusLevelName}", request.Name);
            return Result.Failure<AlertStatusLevelResponse>(
                Error.Conflict("AlertStatusLevel.NameTaken", $"Alert status level '{request.Name}' already exists"));
        }

        // If this is being set as default, clear any existing default
        if (request.IsDefault && !statusLevel.IsDefault)
        {
            var existingDefault = await context.AlertStatusLevels
                .FirstOrDefaultAsync(s => s.IsDefault && s.Id != id, cancellationToken);

            if (existingDefault is not null)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedDate = DateTime.UtcNow;
            }
        }

        // If this is being set as canceled, clear any existing canceled
        if (request.IsCanceled && !statusLevel.IsCanceled)
        {
            var existingCanceled = await context.AlertStatusLevels
                .FirstOrDefaultAsync(s => s.IsCanceled && s.Id != id, cancellationToken);

            if (existingCanceled is not null)
            {
                existingCanceled.IsCanceled = false;
                existingCanceled.UpdatedDate = DateTime.UtcNow;
            }
        }

        statusLevel.Name = request.Name;
        statusLevel.Description = request.Description;
        statusLevel.Color = request.Color;
        statusLevel.SendNotifications = request.SendNotifications;
        statusLevel.IsDefault = request.IsDefault;
        statusLevel.IsCanceled = request.IsCanceled;
        statusLevel.UpdatedDate = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert status level {AlertStatusLevelId} updated successfully", id);

        return Result.Success(MapToResponse(statusLevel));
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting alert status level {AlertStatusLevelId}", id);

        var statusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (statusLevel is null)
        {
            logger.LogWarning("Alert status level not found: {AlertStatusLevelId}", id);
            return Result.Failure(
                Error.NotFound("AlertStatusLevel.NotFound", $"Alert status level with ID {id} not found"));
        }

        var hasAlerts = await context.Alerts
            .AnyAsync(a => a.AlertStatusLevelId == id, cancellationToken);

        if (hasAlerts)
        {
            logger.LogWarning("Alert status level {AlertStatusLevelId} is referenced by existing alerts", id);
            return Result.Failure(
                Error.Conflict("AlertStatusLevel.InUse", "Cannot delete alert status level that is referenced by existing alerts"));
        }

        context.AlertStatusLevels.Remove(statusLevel);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert status level {AlertStatusLevelId} deleted successfully", id);

        return Result.Success();
    }

    private static AlertStatusLevelResponse MapToResponse(AlertStatusLevel statusLevel)
    {
        return new AlertStatusLevelResponse(
            statusLevel.Id,
            statusLevel.Name,
            statusLevel.Description,
            statusLevel.Color,
            statusLevel.SendNotifications,
            statusLevel.IsDefault,
            statusLevel.IsCanceled,
            statusLevel.CreatedDate,
            statusLevel.UpdatedDate);
    }
}
