namespace AlertOne.Core.Abstractions;

/// <summary>
/// Base options for an API client.
/// </summary>
public abstract class ApiOptionsBase
{
    /// <summary>
    /// Gets or sets the base URI of the API.
    /// </summary>
    public string BaseUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function key sent in the <c>x-functions-key</c> header.
    /// </summary>
    public string FunctionKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets the name of the HTTP client used for the API.
    /// </summary>
    public abstract string GetHttpClientName();
}
