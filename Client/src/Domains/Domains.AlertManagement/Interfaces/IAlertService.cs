namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alerts
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Gets all alerts.
    /// </summary>
    Task<IApiResult<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets an alert by identifier.
    /// </summary>
    Task<IApiResult<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an alert.
    /// </summary>
    Task<IApiResult<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rescinds an alert.
    /// </summary>
    Task<IApiResult> RescindAsync(string alertId, RescindAlertRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the event history of an alert.
    /// </summary>
    Task<IApiResult<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Moves an alert to a different status level.
    /// </summary>
    Task<IApiResult<AlertResponse>> UpdateStatusAsync(string alertId, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
