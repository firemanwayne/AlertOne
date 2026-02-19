namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert types
/// </summary>
public interface IAlertTypeService
{
    Task<IApiResult<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> UpdateAsync(Guid id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
