# Aspire

.NET Aspire orchestration for local development. Start **AlertOne.AppHost** (it is the solution's startup project) and everything runs locally: no Azure resources are needed.

| Project | Purpose |
| --- | --- |
| `AlertOne.AppHost` | The orchestrator. Declares the resources below and wires their configuration. |
| `AlertOne.ServiceDefaults` | Shared defaults for hosted projects: OpenTelemetry, health checks (`/health`, `/alive`), service discovery, HTTP client resilience. |
| `AlertOne.ServiceDefaults.Maui` | The same telemetry defaults for the MAUI app. |

## What the AppHost starts

| Resource | Details |
| --- | --- |
| `cosmos` | Azure Cosmos DB preview (vnext) emulator in Docker, persistent container, Data Explorer enabled. Serves plain HTTP, so no certificate setup is needed. |
| `api` | User Management API on `http://localhost:7071`. |
| `alerts-api` | Alert Management API on `http://localhost:7072`. |
| `blazor` | The Blazor WebAssembly app. It waits for both APIs. |
| Functions host storage | Azurite container started automatically for the Functions projects. |

## Configuration injected into the APIs

The AppHost sets these environment variables (replacing what Azure App Configuration supplies in Azure):

- `CosmosDb__Endpoint`, `CosmosDb__AccountKey` (the well-known public emulator key), `CosmosDb__DatabaseName` (`alertone-usrm`, `alertone-alerts`)
- `JwtSettings__usrm__*` for the User Management API (local-only values)

The Blazor app cannot read process environment variables, so its API URLs live in `Client/src/Web/wwwroot/appsettings*.json` and must match the ports above.

## Requirements and notes

- **Docker** must be running (Cosmos emulator and Azurite).
- The first start pulls the emulator image and the emulator takes a minute or two to accept requests. The APIs create the database and containers in the background and retry, so early requests can fail; look for `Cosmos DB ready` in the API logs.
- The `cosmos` resource may show as unhealthy in the dashboard: Aspire's built-in check does not suit the preview emulator. The APIs therefore wait for the container to *start*, not to be healthy.
- Email and push notifications are logged, not sent, because no Communication Services or Notification Hubs connection strings are configured locally.
