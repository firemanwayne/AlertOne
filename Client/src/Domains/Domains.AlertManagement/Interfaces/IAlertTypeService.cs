namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert types
/// </summary>
public interface IAlertTypeService
{
    /// <summary>
    /// Gets all alert types.
    /// </summary>
    Task<IApiResult<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets an alert type by identifier.
    /// </summary>
    Task<IApiResult<AlertTypeResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an alert type.
    /// </summary>
    Task<IApiResult<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an alert type.
    /// </summary>
    Task<IApiResult<AlertTypeResponse>> UpdateAsync(string id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes an alert type.
    /// </summary>
    Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
