using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

public interface IAlertService
{
    Task<Result<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertResponse?>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<Result> RescindAsync(string id, RescindAlertRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> UpdateStatusAsync(string id, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
