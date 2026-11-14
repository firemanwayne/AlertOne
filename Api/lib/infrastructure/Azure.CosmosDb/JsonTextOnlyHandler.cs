namespace Azure.CosmosDb;

/// <summary>
/// Rewrites the Cosmos DB serialization format header so the server always answers with plain JSON.
/// The Cosmos DB vNext emulator replies in the binary format when it is offered, and its encoding of
/// system properties such as <c>_rid</c> cannot be read by the .NET SDK (<c>Unknown JsonNodeType</c>).
/// </summary>
/// <param name="innerHandler">The handler that performs the HTTP request.</param>
internal sealed class JsonTextOnlyHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    /// <summary>
    /// The header the SDK uses to list the response formats it accepts.
    /// </summary>
    private const string SupportedFormatsHeader = "x-ms-cosmos-supported-serialization-formats";

    /// <summary>
    /// The only format offered to the server.
    /// </summary>
    private const string JsonTextFormat = "JsonText";

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Contains(SupportedFormatsHeader))
        {
            request.Headers.Remove(SupportedFormatsHeader);

            request.Headers.TryAddWithoutValidation(SupportedFormatsHeader, JsonTextFormat);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
