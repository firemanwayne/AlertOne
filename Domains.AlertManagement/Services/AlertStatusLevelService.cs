namespace Domains.AlertManagement.Services;

internal sealed class AlertStatusLevelService(AlertManagementApiClient apiClient) : IAlertStatusLevelService
{
    public Task<IApiResult<IEnumerable<AlertStatusLevelResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AlertStatusLevelResponse>>("/api/alert-status-levels", cancellationToken);

    public Task<IApiResult<AlertStatusLevelResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<AlertStatusLevelResponse>($"/api/alert-status-levels/{id}", cancellationToken);

    public Task<IApiResult<AlertStatusLevelResponse>> CreateAsync(CreateAlertStatusLevelRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<CreateAlertStatusLevelRequest, AlertStatusLevelResponse>("/api/alert-status-levels", request, cancellationToken);

    public Task<IApiResult<AlertStatusLevelResponse>> UpdateAsync(Guid id, UpdateAlertStatusLevelRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<UpdateAlertStatusLevelRequest, AlertStatusLevelResponse>($"/api/alert-status-levels/{id}", request, cancellationToken);

    public Task<IApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"/api/alert-status-levels/{id}", cancellationToken);
}
