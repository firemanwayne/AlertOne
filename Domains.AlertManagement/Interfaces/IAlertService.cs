using Domains.AlertManagement.AlertAggregate.Dtos;
using Domains.AlertManagement.AlertAggregate.Requests;
using UserManagementApps.Core.Common;

namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alerts
/// </summary>
public interface IAlertService
{
    Task<IApiResult<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> RescindAsync(Guid alertId, RescindAlertRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertResponse>> UpdateStatusAsync(Guid alertId, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
