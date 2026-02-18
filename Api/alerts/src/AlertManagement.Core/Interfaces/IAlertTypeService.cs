using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

public interface IAlertTypeService
{
    Task<Result<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<Result<AlertTypeResponse>> UpdateAsync(Guid id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
