using Domains.AlertManagement.AlertStatusLevelAggregate.Dtos;
using Domains.AlertManagement.AlertStatusLevelAggregate.Requests;
using UserManagementApps.Core.Common;

namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert status levels
/// </summary>
public interface IAlertStatusLevelService
{
    Task<IApiResult<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> UpdateAsync(Guid id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
