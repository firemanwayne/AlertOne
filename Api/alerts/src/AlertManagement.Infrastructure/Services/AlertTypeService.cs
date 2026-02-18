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
/// Service for alert type management operations
/// </summary>
internal sealed class AlertTypeService(AlertDbContext context, ILogger<AlertTypeService> logger) : IAlertTypeService
{
    public async Task<Result<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all alert types");

        var alertTypes = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var responses = alertTypes.Select(MapToResponse);

        return Result.Success(responses);
    }

    public async Task<Result<AlertTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting alert type {AlertTypeId}", id);

        var alertType = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure<AlertTypeResponse>(
                Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        return Result.Success(MapToResponse(alertType));
    }

    public async Task<Result<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating alert type: {AlertTypeName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertTypeResponse>(
                Error.Validation("AlertType.NameRequired", "Alert type name is required"));
        }

        var existingType = await context.AlertTypes
            .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);

        if (existingType is not null)
        {
            logger.LogWarning("Alert type already exists: {AlertTypeName}", request.Name);
            return Result.Failure<AlertTypeResponse>(
                Error.Conflict("AlertType.AlreadyExists", $"Alert type '{request.Name}' already exists"));
        }

        var alertType = new AlertType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        context.AlertTypes.Add(alertType);

        if (request.NotifyUserIds.Count > 0)
        {
            var alertTypeUsers = request.NotifyUserIds
                .Select(userId => new AlertTypeUser { AlertTypeId = alertType.Id, UserId = userId });

            context.AlertTypeUsers.AddRange(alertTypeUsers);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} created successfully", alertType.Id);

        var created = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .FirstAsync(t => t.Id == alertType.Id, cancellationToken);

        return Result.Success(MapToResponse(created));
    }

    public async Task<Result<AlertTypeResponse>> UpdateAsync(Guid id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating alert type {AlertTypeId}", id);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertTypeResponse>(
                Error.Validation("AlertType.NameRequired", "Alert type name is required"));
        }

        var alertType = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure<AlertTypeResponse>(
                Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        var existingType = await context.AlertTypes
            .FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != id, cancellationToken);

        if (existingType is not null)
        {
            logger.LogWarning("Alert type name already taken: {AlertTypeName}", request.Name);
            return Result.Failure<AlertTypeResponse>(
                Error.Conflict("AlertType.NameTaken", $"Alert type '{request.Name}' already exists"));
        }

        alertType.Name = request.Name;
        alertType.Description = request.Description;
        alertType.Color = request.Color;
        alertType.LastModifiedAt = DateTime.UtcNow;

        // Replace notify user list
        context.AlertTypeUsers.RemoveRange(alertType.AlertTypeUsers);

        if (request.NotifyUserIds.Count > 0)
        {
            var alertTypeUsers = request.NotifyUserIds
                .Select(userId => new AlertTypeUser { AlertTypeId = alertType.Id, UserId = userId });

            context.AlertTypeUsers.AddRange(alertTypeUsers);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} updated successfully", id);

        var updated = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .FirstAsync(t => t.Id == id, cancellationToken);

        return Result.Success(MapToResponse(updated));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting alert type {AlertTypeId}", id);

        var alertType = await context.AlertTypes
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure(
                Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        var hasAlerts = await context.Alerts
            .AnyAsync(a => a.AlertTypeId == id, cancellationToken);

        if (hasAlerts)
        {
            logger.LogWarning("Alert type {AlertTypeId} is referenced by existing alerts", id);
            return Result.Failure(
                Error.Conflict("AlertType.InUse", "Cannot delete alert type that is referenced by existing alerts"));
        }

        context.AlertTypes.Remove(alertType);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} deleted successfully", id);

        return Result.Success();
    }

    private static AlertTypeResponse MapToResponse(AlertType alertType)
    {
        var notifyUserIds = alertType.AlertTypeUsers
            .Select(u => u.UserId)
            .ToList();

        return new AlertTypeResponse(
            alertType.Id,
            alertType.Name,
            alertType.Description,
            alertType.Color,
            notifyUserIds,
            alertType.CreatedAt,
            alertType.LastModifiedAt);
    }
}
