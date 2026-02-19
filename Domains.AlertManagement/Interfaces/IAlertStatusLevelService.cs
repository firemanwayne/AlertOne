namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert status levels
/// </summary>
public interface IAlertStatusLevelService
{
    Task<IApiResult<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertStatusLevelResponse>> UpdateAsync(string id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
