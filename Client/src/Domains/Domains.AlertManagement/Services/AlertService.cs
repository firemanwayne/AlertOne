namespace Domains.AlertManagement.Services;

/// <summary>
/// Alert operations implemented by calling the Alert Management API.
/// </summary>
internal sealed class AlertService(IAlertManagementApiClient apiClient) : IAlertService
{
    /// <inheritdoc />
    public Task<IApiResult<IEnumerable<AlertResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertResponse>>("/api/alerts", cancellationToken);

    /// <inheritdoc />
    public Task<IApiResult<AlertResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<AlertResponse>($"/api/alerts/{id}", cancellationToken);

    /// <inheritdoc />
    public Task<IApiResult<AlertResponse>> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<CreateAlertRequest, AlertResponse>("/api/alerts", request, cancellationToken);

    /// <inheritdoc />
    public Task<IApiResult> RescindAsync(string alertId, RescindAlertRequest request, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"/api/alerts/{alertId}/rescind", request, cancellationToken);

    /// <inheritdoc />
    public Task<IApiResult<IEnumerable<AlertHistoryResponse>>> GetHistoryAsync(string alertId, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertHistoryResponse>>($"/api/alerts/{alertId}/history", cancellationToken);

    /// <inheritdoc />
    public Task<IApiResult<AlertResponse>> UpdateStatusAsync(string alertId, UpdateAlertStatusRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<UpdateAlertStatusRequest, AlertResponse>($"/api/alerts/{alertId}/status", request, cancellationToken);
}
