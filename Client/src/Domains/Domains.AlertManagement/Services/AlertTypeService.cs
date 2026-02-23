namespace Domains.AlertManagement.Services;

internal sealed class AlertTypeService(IAlertManagementApiClient apiClient) : IAlertTypeService
{
    public Task<IApiResult<IEnumerable<AlertTypeResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertTypeResponse>>("/api/alert-types", cancellationToken);

    public Task<IApiResult<AlertTypeResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<AlertTypeResponse>($"/api/alert-types/{id}", cancellationToken);

    public Task<IApiResult<AlertTypeResponse>> CreateAsync(CreateAlertTypeRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<CreateAlertTypeRequest, AlertTypeResponse>("/api/alert-types", request, cancellationToken);

    public Task<IApiResult<AlertTypeResponse>> UpdateAsync(string id, UpdateAlertTypeRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<UpdateAlertTypeRequest, AlertTypeResponse>($"/api/alert-types/{id}", request, cancellationToken);

    public Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"/api/alert-types/{id}", cancellationToken);
}
