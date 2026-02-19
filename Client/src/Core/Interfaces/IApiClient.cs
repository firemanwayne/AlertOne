using AlertOne.Core.Common;

namespace AlertOne.Core.Interfaces;

/// <summary>
/// HTTP client for API communication
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends a GET request
    /// </summary>
    Task<IApiResult<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request
    /// </summary>
    Task<IApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request
    /// </summary>
    Task<IApiResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request
    /// </summary>
    Task<IApiResult> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request with a JSON body
    /// </summary>
    Task<IApiResult> DeleteAsync<TRequest>(string endpoint, TRequest body, CancellationToken cancellationToken = default);
}
