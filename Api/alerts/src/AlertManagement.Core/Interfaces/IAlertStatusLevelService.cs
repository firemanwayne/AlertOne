using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

public interface IAlertStatusLevelService
{
    Task<Result<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AlertStatusLevelResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<Result<AlertStatusLevelResponse>> UpdateAsync(Guid id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
