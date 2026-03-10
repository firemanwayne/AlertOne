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
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // Fetch alert type users separately (Cosmos doesn't support Include)
        var alertTypeIds = alertTypes
            .Select(t => t.Id)
            .ToList();

        var alertTypeUsers = await context.AlertTypeUsers
            .Where(u => alertTypeIds.Contains(u.AlertTypeId))
            .ToListAsync(cancellationToken);

        var responses = alertTypes.Select(t => MapToResponse(t, alertTypeUsers.Where(u => u.AlertTypeId == t.Id)));

        return Result.Success(responses);
    }

    public async Task<Result<AlertTypeResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting alert type {AlertTypeId}", id);

        var alertType = await context.AlertTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure<AlertTypeResponse>(Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        var alertTypeUsers = await context.AlertTypeUsers
            .Where(u => u.AlertTypeId == id)
            .ToListAsync(cancellationToken);

        return Result.Success(MapToResponse(alertType, alertTypeUsers));
    }

    public async Task<Result<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating alert type: {AlertTypeName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertTypeResponse>(Error.Validation("AlertType.NameRequired", "Alert type name is required"));
        }

        var existingType = await context.AlertTypes.FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);

        if (existingType is not null)
        {
            logger.LogWarning("Alert type already exists: {AlertTypeName}", request.Name);
            return Result.Failure<AlertTypeResponse>(Error.Conflict("AlertType.AlreadyExists", $"Alert type '{request.Name}' already exists"));
        }

        var alertType = new AlertType
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            BackgroundColor = request.BackgroundColor,
            TextColor = request.TextColor,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        context.AlertTypes.Add(alertType);

        var createdAlertTypeUsers = new List<AlertTypeUser>();

        if (request.NotifyUserIds.Count > 0)
        {
            var alertTypeUsers = request.NotifyUserIds.Select(userId => new AlertTypeUser
            {
                Id = Guid.NewGuid().ToString(),
                AlertTypeId = alertType.Id,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            context.AlertTypeUsers.AddRange(alertTypeUsers);
            createdAlertTypeUsers.AddRange(alertTypeUsers);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} created successfully", alertType.Id);

        return Result.Success(MapToResponse(alertType, createdAlertTypeUsers));
    }

    public async Task<Result<AlertTypeResponse>> UpdateAsync(string id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating alert type {AlertTypeId}", id);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AlertTypeResponse>(Error.Validation("AlertType.NameRequired", "Alert type name is required"));
        }

        var alertType = await context.AlertTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure<AlertTypeResponse>(Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        var existingType = await context.AlertTypes.FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != id, cancellationToken);

        if (existingType is not null)
        {
            logger.LogWarning("Alert type name already taken: {AlertTypeName}", request.Name);
            return Result.Failure<AlertTypeResponse>(Error.Conflict("AlertType.NameTaken", $"Alert type '{request.Name}' already exists"));
        }

        alertType.Name = request.Name;
        alertType.Description = request.Description;
        alertType.BackgroundColor = request.BackgroundColor;
        alertType.TextColor = request.TextColor;
        alertType.UpdatedDate = DateTime.UtcNow;

        // Get existing alert type users
        var existingAlertTypeUsers = await context.AlertTypeUsers
            .Where(u => u.AlertTypeId == id)
            .ToListAsync(cancellationToken);

        // Remove existing alert type users
        foreach (var user in existingAlertTypeUsers)
        {
            context.AlertTypeUsers.Remove(user);
        }

        var updatedAlertTypeUsers = new List<AlertTypeUser>();

        if (request.NotifyUserIds.Count > 0)
        {
            var alertTypeUsers = request.NotifyUserIds.Select(userId => new AlertTypeUser
            {
                Id = Guid.NewGuid().ToString(),
                AlertTypeId = alertType.Id,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            context.AlertTypeUsers.AddRange(alertTypeUsers);
            updatedAlertTypeUsers.AddRange(alertTypeUsers);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} updated successfully", id);

        return Result.Success(MapToResponse(alertType, updatedAlertTypeUsers));
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting alert type {AlertTypeId}", id);

        var alertType = await context.AlertTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (alertType is null)
        {
            logger.LogWarning("Alert type not found: {AlertTypeId}", id);
            return Result.Failure(Error.NotFound("AlertType.NotFound", $"Alert type with ID {id} not found"));
        }

        var hasAlerts = await context.Alerts.AnyAsync(a => a.AlertTypeId == id, cancellationToken);

        if (hasAlerts)
        {
            logger.LogWarning("Alert type {AlertTypeId} is referenced by existing alerts", id);
            return Result.Failure(Error.Conflict("AlertType.InUse", "Cannot delete alert type that is referenced by existing alerts"));
        }

        // Delete associated alert type users first
        var alertTypeUsers = await context.AlertTypeUsers
            .Where(u => u.AlertTypeId == id)
            .ToListAsync(cancellationToken);

        foreach (var user in alertTypeUsers)
        {
            context.AlertTypeUsers.Remove(user);
        }

        context.AlertTypes.Remove(alertType);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert type {AlertTypeId} deleted successfully", id);

        return Result.Success();
    }

    private static AlertTypeResponse MapToResponse(AlertType alertType, IEnumerable<AlertTypeUser> alertTypeUsers)
    {
        var notifyUserIds = alertTypeUsers
            .Select(u => u.UserId)
            .ToList();

        return new AlertTypeResponse(
            alertType.Id,
            alertType.Name,
            alertType.Description,
            alertType.BackgroundColor,
            alertType.TextColor,
            notifyUserIds,
            alertType.CreatedDate,
            alertType.UpdatedDate);
    }
}
