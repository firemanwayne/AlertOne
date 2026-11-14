using AlertManagement.Core.Common;
using AlertManagement.Core.DTOs.Requests;
using AlertManagement.Core.DTOs.Responses;

namespace AlertManagement.Core.Interfaces;

/// <summary>
/// Business operations for alert types.
/// </summary>
public interface IAlertTypeService
{
    /// <summary>
    /// Gets all alert types.
    /// </summary>
    Task<Result<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets an alert type by identifier.
    /// </summary>
    Task<Result<AlertTypeResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an alert type.
    /// </summary>
    Task<Result<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an alert type.
    /// </summary>
    Task<Result<AlertTypeResponse>> UpdateAsync(string id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes an alert type.
    /// </summary>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
