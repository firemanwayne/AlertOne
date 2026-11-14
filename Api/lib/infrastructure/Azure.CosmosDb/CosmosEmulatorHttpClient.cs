namespace Azure.CosmosDb;

/// <summary>
/// Creates the <see cref="HttpClient"/> used to talk to the Cosmos DB emulator.
/// </summary>
public static class CosmosEmulatorHttpClient
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> that only asks the emulator for plain JSON responses.
    /// Pass it to the EF Core Cosmos <c>HttpClientFactory</c> option when connecting with the emulator account key.
    /// </summary>
    public static HttpClient Create()
    {
        return new HttpClient(new JsonTextOnlyHandler(new HttpClientHandler()));
    }
}
