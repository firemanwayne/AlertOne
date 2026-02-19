namespace Domains.AlertManagement.Services;

internal sealed class AlertService(AlertManagementApiClient apiClient) : IAlertService
{
    public Task<IApiResult<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertResponse>>("/api/alerts", cancellationToken);

    public Task<IApiResult<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<AlertResponse>($"/api/alerts/{id}", cancellationToken);

    public Task<IApiResult<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<CreateAlertRequest, AlertResponse>("/api/alerts", request, cancellationToken);

    public Task<IApiResult> RescindAsync(string alertId, RescindAlertRequest request, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"/api/alerts/{alertId}/rescind", request, cancellationToken);

    public Task<IApiResult<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertHistoryResponse>>($"/api/alerts/{alertId}/history", cancellationToken);

    public Task<IApiResult<AlertResponse>> UpdateStatusAsync(string alertId, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<UpdateAlertStatusRequest, AlertResponse>($"/api/alerts/{alertId}/status", request, cancellationToken);
}
