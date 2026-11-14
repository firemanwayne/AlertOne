using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

/// <summary>
/// Business operations for alert status levels.
/// </summary>
public interface IAlertStatusLevelService
{
    /// <summary>
    /// Gets all status levels.
    /// </summary>
    Task<Result<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a status level by identifier.
    /// </summary>
    Task<Result<AlertStatusLevelResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a status level.
    /// </summary>
    Task<Result<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates a status level.
    /// </summary>
    Task<Result<AlertStatusLevelResponse>> UpdateAsync(string id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a status level unless alerts still reference it.
    /// </summary>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
