namespace Domains.AlertManagement.Services;

/// <summary>
/// Typed HTTP client for AlertManagement API communication
/// </summary>
internal sealed class AlertManagementApiClient(HttpClient client) : IAlertManagementApiClient
{
    /// <summary>
    /// JSON options used to read API responses.
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public async Task<IApiResult<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetAsync(endpoint, cancellationToken);

            return await ProcessResponseAsync<TResponse>(response);
        }
        catch (HttpRequestException ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Http.RequestFailed", $"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Api.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<IApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(endpoint, request, cancellationToken);

            return await ProcessResponseAsync<TResponse>(response);
        }
        catch (HttpRequestException ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Http.RequestFailed", $"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Api.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<IApiResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PutAsJsonAsync(endpoint, request, cancellationToken);

            return await ProcessResponseAsync<TResponse>(response);
        }
        catch (HttpRequestException ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Http.RequestFailed", $"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure(
                Error.Internal("Api.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<IApiResult> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.DeleteAsync(endpoint, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = await ParseErrorResponseAsync(response);

            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(
                Error.Internal("Http.RequestFailed", $"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure(
                Error.Internal("Api.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<IApiResult> DeleteAsync<TRequest>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, endpoint)
            {
                Content = JsonContent.Create(body)
            };

            var response = await client.SendAsync(requestMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = await ParseErrorResponseAsync(response);

            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(
                Error.Internal("Http.RequestFailed", $"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure(
                Error.Internal("Api.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Reads a response and converts it to a typed result, mapping failures to an error.
    /// </summary>
    private async Task<IApiResult<TResponse>> ProcessResponseAsync<TResponse>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);

            if (data is null)
            {
                return Result<TResponse>.Failure(
                    Error.Internal("Api.DeserializationFailed", "Failed to deserialize response"));
            }

            return Result<TResponse>.Success(data);
        }

        var error = await ParseErrorResponseAsync(response);

        return Result<TResponse>.Failure(error);
    }

    /// <summary>
    /// Reads the API error body (code and message) from a failed response.
    /// </summary>
    private async Task<Error> ParseErrorResponseAsync(HttpResponseMessage response)
    {
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var code = $"Http.{response.StatusCode}";
            var message = string.IsNullOrWhiteSpace(errorContent)
                ? response.ReasonPhrase ?? "Unknown error"
                : errorContent;

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => Error.Validation(code, message),
                System.Net.HttpStatusCode.NotFound => Error.NotFound(code, message),
                System.Net.HttpStatusCode.Conflict => Error.Conflict(code, message),
                System.Net.HttpStatusCode.Unauthorized => Error.Unauthorized(code, message),
                System.Net.HttpStatusCode.Forbidden => Error.Forbidden(code, message),
                _ => Error.Internal(code, message)
            };
        }
        catch
        {
            return Error.Internal(
                $"Http.{response.StatusCode}",
                response.ReasonPhrase ?? "Unknown error");
        }
    }
}
