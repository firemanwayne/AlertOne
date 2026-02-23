using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;
using AlertManagement.Core.Entities;
using AlertManagement.Core.Interfaces;
using AlertManagement.Infrastructure.Data;
using AlertOne.Api.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlertManagement.Infrastructure.Services;

/// <summary>
/// Service for alert management operations
/// </summary>
internal sealed class AlertService(
    AlertDbContext context,
    ILogger<AlertService> logger,
    IPushNotificationService pushNotificationService) : IAlertService
{
    private const string EventTypeCreated = "Created";
    private const string EventTypeRescinded = "Rescinded";
    private const string EventTypeStatusUpdated = "StatusUpdated";

    public async Task<Result<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all alerts");

        var alerts = await context.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(t => t.AlertTypeUsers)
            .Include(a => a.AlertStatusLevel)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);

        var responses = alerts.Select(MapToResponse);

        return Result.Success(responses);
    }

    public async Task<Result<AlertResponse?>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting active alert");

        var alert = await context.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(t => t.AlertTypeUsers)
            .Include(a => a.AlertStatusLevel)
            .FirstOrDefaultAsync(a => a.IsActive, cancellationToken);

        if (alert is null)
        {
            return Result.Success<AlertResponse?>(null);
        }

        return Result.Success<AlertResponse?>(MapToResponse(alert));
    }

    public async Task<Result<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating alert for type {AlertTypeId}", request.AlertTypeId);

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            return Result.Failure<AlertResponse>(
                Error.Validation("Alert.CreatedByRequired", "CreatedBy is required"));
        }

        var alertType = await context.AlertTypes
            .Include(t => t.AlertTypeUsers)
            .FirstOrDefaultAsync(t => t.Id == request.AlertTypeId, cancellationToken);

        if (alertType is null)
        {
            return Result.Failure<AlertResponse>(
                Error.NotFound("AlertType.NotFound", $"Alert type with ID {request.AlertTypeId} not found"));
        }

        // Automatically use the default status level
        var statusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.IsDefault, cancellationToken);

        if (statusLevel is null)
        {
            return Result.Failure<AlertResponse>(
                Error.NotFound("AlertStatusLevel.DefaultNotFound", "No default alert status level has been configured"));
        }

        // Deactivate any currently active alert
        var activeAlert = await context.Alerts
            .FirstOrDefaultAsync(a => a.IsActive, cancellationToken);

        if (activeAlert is not null)
        {
            activeAlert.IsActive = false;
            activeAlert.UpdatedDate = DateTime.UtcNow;
            activeAlert.UpdatedBy = request.CreatedBy;
        }

        var now = DateTime.UtcNow;

        var alert = new Alert
        {
            Id = Guid.NewGuid().ToString(),
            AlertTypeId = request.AlertTypeId,
            AlertStatusLevelId = statusLevel.Id,
            CreatedBy = request.CreatedBy,
            CreatedDate = now,
            UpdatedDate = now,
            IsActive = true
        };

        context.Alerts.Add(alert);

        var historyEntry = new AlertHistory
        {
            Id = Guid.NewGuid().ToString(),
            AlertId = alert.Id,
            EventType = EventTypeCreated,
            CreatedBy = request.CreatedBy,
            CreatedDate = now,
            Notes = null
        };

        context.AlertHistories.Add(historyEntry);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert {AlertId} created successfully", alert.Id);

        alert.AlertType = alertType;
        alert.AlertStatusLevel = statusLevel;

        // Send push notifications if the status level has SendNotifications enabled
        await SendNotificationsAsync(alert, alertType, statusLevel, EventTypeCreated, cancellationToken);

        return Result.Success(MapToResponse(alert));
    }

    public async Task<Result> RescindAsync(string id, RescindAlertRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rescinding alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(request.RescindedBy))
        {
            return Result.Failure(
                Error.Validation("Alert.RescindedByRequired", "RescindedBy is required"));
        }

        var alert = await context.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(t => t.AlertTypeUsers)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (alert is null)
        {
            logger.LogWarning("Alert not found: {AlertId}", id);
            return Result.Failure(
                Error.NotFound("Alert.NotFound", $"Alert with ID {id} not found"));
        }

        if (!alert.IsActive)
        {
            return Result.Failure(
                Error.Validation("Alert.NotActive", "Alert is not currently active and cannot be rescinded"));
        }

        // Find the canceled status level if one exists
        var canceledStatusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.IsCanceled, cancellationToken);

        var now = DateTime.UtcNow;

        alert.IsActive = false;
        alert.RescindedAt = now;
        alert.RescindedBy = request.RescindedBy;
        alert.UpdatedDate = now;
        alert.UpdatedBy = request.RescindedBy;

        // Update to canceled status level if one is configured
        if (canceledStatusLevel is not null)
        {
            alert.AlertStatusLevelId = canceledStatusLevel.Id;
        }

        var historyEntry = new AlertHistory
        {
            Id = Guid.NewGuid().ToString(),
            AlertId = alert.Id,
            EventType = EventTypeRescinded,
            CreatedBy = request.RescindedBy,
            CreatedDate = now,
            Notes = null
        };

        context.AlertHistories.Add(historyEntry);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert {AlertId} rescinded successfully by {RescindedBy}", id, request.RescindedBy);

        // Send push notifications if the canceled status level has SendNotifications enabled
        if (canceledStatusLevel is not null)
        {
            await SendNotificationsAsync(alert, alert.AlertType, canceledStatusLevel, EventTypeRescinded, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<AlertResponse>> UpdateStatusAsync(string id, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating status for alert {AlertId}", id);

        if (string.IsNullOrWhiteSpace(request.UpdatedBy))
        {
            return Result.Failure<AlertResponse>(
                Error.Validation("Alert.UpdatedByRequired", "UpdatedBy is required"));
        }

        var alert = await context.Alerts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (alert is null)
        {
            logger.LogWarning("Alert not found: {AlertId}", id);
            return Result.Failure<AlertResponse>(
                Error.NotFound("Alert.NotFound", $"Alert with ID {id} not found"));
        }

        if (!alert.IsActive)
        {
            return Result.Failure<AlertResponse>(
                Error.Validation("Alert.NotActive", "Alert is not currently active and cannot have its status updated"));
        }

        var statusLevel = await context.AlertStatusLevels
            .FirstOrDefaultAsync(s => s.Id == request.AlertStatusLevelId, cancellationToken);

        if (statusLevel is null)
        {
            return Result.Failure<AlertResponse>(
                Error.NotFound("AlertStatusLevel.NotFound", $"Alert status level with ID {request.AlertStatusLevelId} not found"));
        }

        var now = DateTime.UtcNow;

        alert.AlertStatusLevelId = request.AlertStatusLevelId;
        alert.UpdatedDate = now;
        alert.UpdatedBy = request.UpdatedBy;

        var historyEntry = new AlertHistory
        {
            Id = Guid.NewGuid().ToString(),
            AlertId = alert.Id,
            EventType = EventTypeStatusUpdated,
            UpdatedBy = request.UpdatedBy,
            UpdatedDate = now,
            Notes = null
        };

        context.AlertHistories.Add(historyEntry);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Alert {AlertId} status updated to {StatusLevelId} by {UpdatedBy}", id, request.AlertStatusLevelId, request.UpdatedBy);

        var updatedAlert = await context.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(t => t.AlertTypeUsers)
            .Include(a => a.AlertStatusLevel)
            .FirstAsync(a => a.Id == id, cancellationToken);

        // Send push notifications if the new status level has SendNotifications enabled
        await SendNotificationsAsync(updatedAlert, updatedAlert.AlertType, statusLevel, EventTypeStatusUpdated, cancellationToken);

        return Result.Success(MapToResponse(updatedAlert));
    }

    public async Task<Result<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting alert {AlertId}", id);

        var alert = await context.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(t => t.AlertTypeUsers)
            .Include(a => a.AlertStatusLevel)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (alert is null)
        {
            logger.LogWarning("Alert not found: {AlertId}", id);
            return Result.Failure<AlertResponse>(
                Error.NotFound("Alert.NotFound", $"Alert with ID {id} not found"));
        }

        return Result.Success(MapToResponse(alert));
    }

    public async Task<Result<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting history for alert {AlertId}", alertId);

        var alertExists = await context.Alerts
            .AnyAsync(a => a.Id == alertId, cancellationToken);

        if (!alertExists)
        {
            logger.LogWarning("Alert not found when fetching history: {AlertId}", alertId);
            return Result.Failure<IEnumerable<AlertHistoryResponse>>(
                Error.NotFound("Alert.NotFound", $"Alert with ID {alertId} not found"));
        }

        var history = await context.AlertHistories
            .Where(h => h.AlertId == alertId)
            .OrderByDescending(h => h.UpdatedDate)
            .ToListAsync(cancellationToken);

        var responses = history.Select(MapHistoryToResponse);

        return Result.Success(responses);
    }

    private static AlertHistoryResponse MapHistoryToResponse(AlertHistory history) =>
        new(history.Id, history.AlertId, history.EventType, history.UpdatedBy, history.UpdatedDate, history.Notes);

    private static AlertResponse MapToResponse(Alert alert)
    {
        var alertType = alert.AlertType;
        var notifyUserIds = alertType.AlertTypeUsers
            .Select(u => u.UserId)
            .ToList();

        var typeResponse = new AlertTypeResponse(
            alertType.Id,
            alertType.Name,
            alertType.Description,
            alertType.Color,
            notifyUserIds,
            alertType.CreatedDate,
            alertType.UpdatedDate);

        var statusLevel = alert.AlertStatusLevel;
        var statusLevelResponse = new AlertStatusLevelResponse(
            statusLevel.Id,
            statusLevel.Name,
            statusLevel.Description,
            statusLevel.Color,
            statusLevel.SendNotifications,
            statusLevel.IsDefault,
            statusLevel.IsCanceled,
            statusLevel.CreatedDate,
            statusLevel.UpdatedDate);

        return new AlertResponse(
            alert.Id,
            typeResponse,
            statusLevelResponse,
            alert.CreatedBy,
            alert.UpdatedBy,
            alert.CreatedDate,
            alert.UpdatedDate,
            alert.IsActive,
            alert.RescindedAt,
            alert.RescindedBy);
    }

    private async Task SendNotificationsAsync(Alert alert, AlertType alertType, AlertStatusLevel statusLevel, string eventType, CancellationToken cancellationToken)
    {
        if (!statusLevel.SendNotifications)
        {
            logger.LogInformation("Skipping notifications for alert {AlertId} - SendNotifications is false for status level {StatusLevelName}", alert.Id, statusLevel.Name);
            return;
        }

        var userIds = alertType.AlertTypeUsers
            .Select(u => u.UserId)
            .ToList();

        if (userIds.Count == 0)
        {
            logger.LogInformation("No users to notify for alert {AlertId} - AlertType {AlertTypeName} has no assigned users", alert.Id, alertType.Name);
            return;
        }

        logger.LogInformation("Sending push notifications for alert {AlertId} to {UserCount} users", alert.Id, userIds.Count);

        var title = eventType switch
        {
            EventTypeCreated => $"Alert: {alertType.Name}",
            EventTypeStatusUpdated => $"Alert Update: {statusLevel.Name}",
            EventTypeRescinded => "Alert Rescinded",
            _ => "Alert Notification"
        };

        var body = eventType switch
        {
            EventTypeCreated => $"A new {alertType.Name} alert has been issued. Status: {statusLevel.Name}",
            EventTypeStatusUpdated => $"Alert status updated to {statusLevel.Name}",
            EventTypeRescinded => $"The {alertType.Name} alert has been rescinded",
            _ => $"Alert update for {alertType.Name}"
        };

        var data = new Dictionary<string, string>
        {
            { "alertId", alert.Id },
            { "alertTypeId", alertType.Id },
            { "alertTypeName", alertType.Name },
            { "statusLevelId", statusLevel.Id },
            { "statusLevelName", statusLevel.Name },
            { "eventType", eventType },
            { "severity", statusLevel.IsCanceled ? "success" : "warning" },
            { "action", "view_alert" }
        };

        // Send notification to each user
        foreach (var userId in userIds)
        {
            try
            {
                var success = await pushNotificationService.SendToUserAsync(userId, title, body, data, cancellationToken);

                if (success)
                {
                    logger.LogDebug("Push notification sent to user {UserId} for alert {AlertId}", userId, alert.Id);
                }
                else
                {
                    logger.LogWarning("Failed to send push notification to user {UserId} for alert {AlertId}", userId, alert.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending push notification to user {UserId} for alert {AlertId}", userId, alert.Id);
            }
        }

        logger.LogInformation("Push notification dispatch completed for alert {AlertId}", alert.Id);
    }
}
