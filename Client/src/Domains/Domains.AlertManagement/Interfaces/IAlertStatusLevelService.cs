namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert status levels
/// </summary>
public interface IAlertStatusLevelService
{
    /// <summary>
    /// Gets all alert status levels.
    /// </summary>
    Task<IApiResult<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets an alert status level by identifier.
    /// </summary>
    Task<IApiResult<AlertStatusLevelResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an alert status level.
    /// </summary>
    Task<IApiResult<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an alert status level.
    /// </summary>
    Task<IApiResult<AlertStatusLevelResponse>> UpdateAsync(string id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes an alert status level.
    /// </summary>
    Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
