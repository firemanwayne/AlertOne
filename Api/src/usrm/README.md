# Api/src/usrm

The **User Management API**: registration, login and token refresh, users, roles, invitations, devices and user settings, built on ASP.NET Core Identity with custom Cosmos DB stores. Runs as an Azure Functions isolated worker; under Aspire it is the `api` resource on `http://localhost:7071`.

## Projects

| Project | Layer | Contents |
| --- | --- | --- |
| `src/UserManagement.Core` | Domain | `User`, `UserInvitation`, value objects (`Email`, `UserId`, `UserRole`), DTOs, service interfaces, `Result`/`Error`. No external dependencies. |
| `src/UserManagement.Infrastructure` | Implementation | `ApplicationDbContext`, `CosmosUserStore`/`CosmosRoleStore`, `TokenService`, the service implementations, JWT setup, `AddInfrastructure()`. |
| `src/UserManagement.Client` | Host | Azure Functions HTTP triggers, middleware, `Program.cs` (also seeds the default roles). |
| `tests/UnitTests`, `tests/IntegrationTests` | Tests | Unit tests per layer and HTTP-file based integration tests. |

## Endpoints

| Area | Routes |
| --- | --- |
| Auth | `POST auth/register`, `auth/login`, `auth/refresh`, `GET auth/has-users` |
| Users | `GET/POST users`, `GET/PUT/DELETE users/{id}` |
| User settings | `GET/PUT users/{userId}/settings` |
| Roles | `GET/POST roles`, `GET/PUT/DELETE roles/{id}`, `GET roles/name/{name}`, `POST roles/assign`, `POST roles/remove`, `GET roles/{name}/users` |
| Invitations | `GET/POST invitations`, `GET/DELETE invitations/{id}`, `POST invitations/{id}/resend`, `POST invitations/accept`, `GET invitations/validate/{code}` |
| Devices | `GET devices`, `GET/DELETE devices/{id}`, `DELETE devices/all`, `DELETE devices/all-except/{id}`, `PUT devices/{id}/trust`, `PUT devices/{id}/name`, `GET users/{userId}/devices/notifications` |
| Health | `GET health/ping`, `health/live`, `health/ready` |

Route strings are constants on each Functions class.

## Configuration

| Key | Purpose |
| --- | --- |
| `CosmosDb:Endpoint`, `CosmosDb:DatabaseName` | Required. |
| `CosmosDb:AccountKey` | Set for the Cosmos emulator; leave empty to use managed identity in Azure. |
| `JwtSettings:usrm:Secret`, `Issuer`, `Audience`, `ExpiryMinutes` | Required for signing and validating access tokens. |
| `AzureCommunications:*` | Invitation emails; a logging-only stand-in is used when no connection string is set. |
| `AppConfig:BaseUri` | Optional Azure App Configuration. |

Under Aspire these are injected as environment variables by [`Aspire/AlertOne.AppHost`](../../../Aspire).

## Run

Start the **AlertOne.AppHost** project. The Cosmos database and containers are created in the background on first start, so allow a minute or two before the first login.
