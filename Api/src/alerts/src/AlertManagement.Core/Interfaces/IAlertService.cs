using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

/// <summary>
/// Business operations for alerts.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Gets all alerts.
    /// </summary>
    Task<Result<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the currently active alert, if any.
    /// </summary>
    Task<Result<AlertResponse?>> GetActiveAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets an alert by identifier.
    /// </summary>
    Task<Result<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an alert and notifies the users of its type.
    /// </summary>
    Task<Result<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rescinds an alert.
    /// </summary>
    Task<Result> RescindAsync(string id, RescindAlertRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the event history of an alert.
    /// </summary>
    Task<Result<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Moves an alert to a different status level.
    /// </summary>
    Task<Result<AlertResponse>> UpdateStatusAsync(string id, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
