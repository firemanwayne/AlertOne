# Api/src/alerts

The **Alert Management API**: creates and tracks alerts, their types and status levels, and sends push notifications. Runs as an Azure Functions isolated worker; under Aspire it is the `alerts-api` resource on `http://localhost:7072`.

## Projects (`src/`)

| Project | Layer | Contents |
| --- | --- | --- |
| `AlertManagement.Core` | Domain | Entities (`Alert`, `AlertType`, `AlertTypeUser`, `AlertStatusLevel`, `AlertHistory`), request/response DTOs, service interfaces, `Result`/`Error`. No external dependencies. |
| `AlertManagement.Infrastructure` | Implementation | `AlertDbContext` (EF Core on Cosmos DB), the service implementations, `AddInfrastructure()`. |
| `AlertManagement.Client` | Host | Azure Functions HTTP triggers, middleware, `Program.cs`. |

## Endpoints

| Area | Routes |
| --- | --- |
| Alerts | `GET alerts`, `POST alerts`, `GET alerts/active`, `GET alerts/{id}`, `GET alerts/{id}/history`, `PUT alerts/{id}/status`, `DELETE alerts/{id}/rescind` |
| Alert types | `GET/POST alert-types`, `GET/PUT/DELETE alert-types/{id}` |
| Status levels | `GET/POST alert-status-levels`, `GET/PUT/DELETE alert-status-levels/{id}` |
| Notifications | `POST notifications/register`, `notifications/unregister/{registrationId}`, `notifications/send/user/{userId}`, `notifications/send/all`, `notifications/send/tags`, `notifications/test` |
| Health | `GET health/ping`, `health/live`, `health/ready` |

Route strings are constants on each Functions class.

## Configuration

| Key | Purpose |
| --- | --- |
| `CosmosDb:Endpoint`, `CosmosDb:DatabaseName` | Required. |
| `CosmosDb:AccountKey` | Set for the Cosmos emulator; leave empty to use managed identity in Azure. |
| `AzureNotificationHubs:ConnectionString`, `HubName` | Push notifications; a logging-only stand-in is used when empty. |
| `AppConfig:BaseUri` | Optional Azure App Configuration. |

Under Aspire these are injected as environment variables by [`Aspire/AlertOne.AppHost`](../../../Aspire).

## API documentation (OpenAPI / Swagger)

The API describes itself with OpenAPI 3 through `Microsoft.Azure.Functions.Worker.Extensions.OpenApi`. Every function carries `[OpenApiOperation]`, parameter, request body and response attributes, so the document lists each route with its request and response types and the error responses it can return.

| URL | Content |
| --- | --- |
| `http://localhost:7072/api/swagger/ui` | Swagger UI, to read and try the endpoints |
| `http://localhost:7072/api/swagger.json` | OpenAPI document (also `/api/openapi/v3.json`) |

Under Aspire this API is on port 7072.

The document title and description live in `OpenApi/OpenApiConfiguration.cs`. The extension registers itself, so no call in `Program.cs` is needed. When you add a function, add the same `[OpenApi*]` attributes above its `[Function]` attribute so it shows up with the right types.

## Run

Start the **AlertOne.AppHost** project. It starts the Cosmos emulator (Docker is required), this API, the User Management API and the Blazor app.
