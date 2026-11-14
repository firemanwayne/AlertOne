# Api/lib/infrastructure

Adapters between the APIs and external services. Each library registers itself with a `ServiceRegistryExtensions` extension method and is configured through `IConfiguration`.

| Library | Registers / provides | Configuration | When nothing is configured |
| --- | --- | --- | --- |
| `Azure.AppConfig` | `AddAppConfiguration()` adds Azure App Configuration (with Key Vault references and a `Sentinel` key refresh) as a configuration source. | `AppConfig:BaseUri` | Skipped, so local development runs on local settings only. |
| `Azure.Communications` | `AddAzureCommunications()` registers `IEmailService` (Azure Communication Services email). | `AzureCommunications:ConnectionString`, `SenderAddress`, `WebAppBaseUrl`, `MobileAppScheme` | `LoggingEmailService` logs the email (including the invitation activation code) instead of sending it. |
| `Azure.NotificationHubs` | `AddAzureNotificationHubs()` registers `IPushNotificationService` (FCM, APNs and WNS registrations and sends). | `AzureNotificationHubs:ConnectionString`, `HubName` | `LoggingPushNotificationService` logs the push instead of sending it. |
| `Azure.CosmosDb` | `AddCosmosInitialization<TContext>()` creates the Cosmos DB database and containers if they do not exist, in the background, retrying while Cosmos DB (or the emulator) starts. An optional callback runs afterwards, for example to seed data. | None (uses the context's own Cosmos configuration) | Not applicable. |

## Usage

```csharp
builder.Configuration.AddAppConfiguration();            // optional, needs AppConfig:BaseUri

builder.Services
    .AddAzureCommunications(builder.Configuration)      // user management API
    .AddAzureNotificationHubs(builder.Configuration)    // alert management API
    .AddCosmosInitialization<MyDbContext>();            // both APIs
```

## Notes

- Do **not** reference `Azure.AppConfig` from the Blazor WebAssembly client. The provider blocks while loading and `DefaultAzureCredential` cannot run in a browser.
- Registrations use `TryAdd*`, so a host can pre-register its own implementation (for example in tests).
- `CosmosEmulatorHttpClient.Create()` is used by both APIs when connecting with the emulator account key. It makes the SDK ask the Cosmos DB vNext emulator for plain JSON, because the emulator's binary responses cannot be read by the .NET SDK (any query returning whole documents fails with `Unknown JsonNodeType` on the `_rid` field).
- The Cosmos initializer runs after the host starts, so API calls made in the first moments of a cold start can fail until the database exists. Watch the log for `Cosmos DB ready`.
