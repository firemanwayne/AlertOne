namespace Domains.AlertManagement.Interfaces;

/// <summary>
/// Service for managing alert types
/// </summary>
public interface IAlertTypeService
{
    Task<IApiResult<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult<AlertTypeResponse>> UpdateAsync(string id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default);
    Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
