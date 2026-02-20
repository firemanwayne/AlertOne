namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alerts
/// </summary>
public interface IAlertService
{
    Task<IApiResult<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> RescindAsync(string alertId, RescindAlertRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> UpdateStatusAsync(string alertId, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
