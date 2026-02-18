using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

public interface IAlertService
{
    Task<Result<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertResponse?>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<Result> RescindAsync(Guid id, RescindAlertRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<Result<AlertResponse>> UpdateStatusAsync(Guid id, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default);
}
