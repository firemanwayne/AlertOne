# AlertOne

A comprehensive emergency alert management system built with .NET 9, featuring a cross-platform mobile app (MAUI), web dashboard (Blazor), and serverless backend (Azure Functions).

## Overview

AlertOne enables organizations to create, manage, and broadcast emergency alerts to their teams. The system supports real-time push notifications to mobile devices, role-based access control, and a complete audit trail of alert activities.

## Architecture

```
AlertOne/
├── Api/                          # Backend Azure Functions
│   ├── alerts/                   # Alert Management API
│   │   └── src/
│   │       ├── AlertManagement.Client/      # Azure Functions endpoints
│   │       ├── AlertManagement.Core/        # Domain models, DTOs, interfaces
│   │       └── AlertManagement.Infrastructure/  # EF Core (Cosmos DB), services
│   ├── usrm/                     # User Management API
│   │   └── src/
│   │       ├── UserManagement.Client/       # Azure Functions endpoints
│   │       ├── UserManagement.Core/         # Domain models, DTOs, interfaces
│   │       └── UserManagement.Infrastructure/   # Identity, EF Core (Cosmos DB), services
│   ├── core/                     # Shared API core library
│   ├── infrastructure/           # Shared infrastructure libraries
│   │   ├── Azure.AppConfig/      # Azure App Configuration
│   │   ├── Azure.Communications/ # Azure Communication Services (Email)
│   │   ├── Azure.CosmosDb/       # Cosmos DB database/container initialization
│   │   └── Azure.NotificationHubs/  # Push notifications
│   └── tests/                    # Integration & unit tests
├── Client/                       # Frontend applications
│   └── src/
│       ├── Core/                 # Shared DTOs, interfaces, models
│       ├── Blazor/               # Shared Blazor components, layouts (AlertOne.Shared.Blazor)
│       ├── Domains/              # Feature modules
│       │   ├── Domains.AlertManagement/   # Alert management UI
│       │   └── Domains.UserManagement/    # User management UI
│       ├── Maui/                 # .NET MAUI mobile app (Android/iOS)
│       └── Web/                  # Blazor WebAssembly web app
├── Aspire/                       # .NET Aspire orchestration
│   ├── AlertOne.AppHost/         # Development orchestrator
│   └── AlertOne.ServiceDefaults/ # Shared service configuration
└── .github/workflows/            # CI/CD pipelines
```

## Tech Stack

### Backend
- **.NET 9** - Latest .NET runtime
- **Azure Functions** (Isolated Worker) - Serverless compute
- **Entity Framework Core 9** - Database ORM
- **Azure Cosmos DB** - Document storage (Cosmos DB emulator in local development)
- **Azure Notification Hubs** - Cross-platform push notifications
- **Azure App Configuration** - Centralized configuration with Key Vault integration
- **Azure Communication Services** - Email notifications

### Frontend
- **Blazor** - Component-based UI framework
- **.NET MAUI** - Cross-platform mobile development
- **MudBlazor** - Material Design component library

### Infrastructure
- **.NET Aspire** - Cloud-native development orchestration
- **GitHub Actions** - CI/CD pipelines
- **Azure** - Cloud hosting

## Features

- **Alert Management**
  - Create and broadcast alerts by type
  - Update alert status levels
  - View alert history and audit trail
  - Rescind active alerts

- **Push Notifications**
  - Real-time notifications to mobile devices
  - Support for Android (FCM) and iOS (APNs)
  - Tag-based targeting (by role, team, location)
  - User-specific notifications
  - In-app notification bell with unread count
  - Notification history page
  - Automatic notifications on alert creation and status updates

- **User Management**
  - JWT-based authentication
  - Role-based authorization
  - User registration and profile management
  - User invitation system with email activation
  - User settings and preferences

- **User Settings**
  - Email notification preferences
  - Push notification preferences
  - Theme customization (placeholder for future)

## Getting Started

AlertOne runs **entirely on your machine**. No Azure subscription or Azure login is needed: .NET Aspire starts both APIs, the web app and local emulators for the data stores.

### Prerequisites

| Requirement | Why | Notes |
| --- | --- | --- |
| [.NET SDK 9.0 or newer](https://dotnet.microsoft.com/download) | Build and run everything | `global.json` asks for 9.0.100 and rolls forward to a newer installed SDK. Check with `dotnet --list-sdks`. |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) (running) | Cosmos DB emulator and Azurite (Functions storage) run as containers | The first start downloads the images, which can take a few minutes. |
| [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) | Provides the Functions host that runs both APIs | Check with `func --version`. |
| Visual Studio 2022 (17.12 or newer) or 2026, **or** VS Code / the `dotnet` CLI | Editor | Visual Studio needs the *ASP.NET and web development* workload. |
| A modern browser | The Blazor web app | |

**Only for the mobile/desktop app (optional):** the .NET MAUI workload (`dotnet workload install maui`) and, for Android, the Android SDK and an emulator or device. The web app does not need them.

**Not needed locally:** Azure account, Azure CLI, SQL Server, Azure App Configuration, Key Vault, Notification Hubs or Communication Services.

### Run the app

1. **Clone and restore**
   ```bash
   git clone https://github.com/firemanwayne/AlertOne.git
   cd AlertOne
   dotnet restore AlertOne.Aspire.slnx
   ```

2. **Start Docker Desktop** and wait until it reports it is running.

3. **Start the AppHost.** Pick one:
   - **Visual Studio:** open `AlertOne.Aspire.slnx`, set **AlertOne.AppHost** as the startup project and press **F5** (or Ctrl+F5).
   - **Command line:**
     ```bash
     dotnet run --project Aspire/AlertOne.AppHost
     ```

4. **Wait for the resources to become ready.** The Aspire dashboard opens in your browser (`https://localhost:17291` with the `https` launch profile). On the very first run the Cosmos DB emulator needs a minute or two to start, and the APIs create their database and containers in the background and retry until it is ready. Look for `Cosmos DB ready` in the `api` and `alerts-api` logs.

5. **Open the web app** from the `blazor` resource link in the dashboard (the web app is served at `https://localhost:7183`).

6. **Create the first account.** When no users exist yet, the login page shows a **Create First User** button. Registering assigns the `User` role. The `Admin`, `Manager` and `User` roles are created automatically on first start and can be assigned through the roles endpoints (`POST /roles/assign`).

### What starts

| Resource | Address | Purpose |
| --- | --- | --- |
| Aspire dashboard | `https://localhost:17291` | Logs, traces, metrics and resource status |
| `blazor` | `https://localhost:7183` | Web app |
| `api` | `http://localhost:7071` | User Management API |
| `alerts-api` | `http://localhost:7072` | Alert Management API |
| `cosmos` | assigned by Aspire (Data Explorer link in the dashboard) | Cosmos DB emulator |
| Functions storage | assigned by Aspire | Azurite emulator |

How the pieces are wired together is described in [`Aspire/README.md`](./Aspire/README.md).

### Local behavior to expect

- **Email and push notifications are logged, not sent.** With no Azure Communication Services or Notification Hubs connection strings configured, the APIs use logging-only stand-ins. Invitation activation codes appear in the `api` log.
- The `cosmos` resource may show as *unhealthy* in the dashboard. The APIs wait for the container to start, not for that check, so this is cosmetic.
- The Cosmos container is persistent, so your data survives restarts. To start clean, stop the AppHost and remove the `cosmos-*` container in Docker.

### Troubleshooting

| Symptom | Likely cause and fix |
| --- | --- |
| AppHost fails immediately or the `cosmos` container never starts | Docker Desktop is not running. Start it and rerun. |
| Login times out right after the first start | Cosmos is still initializing. Wait for `Cosmos DB ready` in the API logs and retry. |
| Port 7071 or 7072 is already in use | Another Functions host is running. Stop it; the AppHost pins these ports and the web app's `wwwroot/appsettings*.json` depends on them. |
| The web app shows `An unhandled error has occurred` | Open the browser console. The API URLs come from `Client/src/Web/wwwroot/appsettings.Development.json` and must match the ports above. |
| `Cannot wait on monitors on this runtime` | An Azure SDK or blocking configuration provider was added to the WebAssembly project. Keep Azure SDKs out of `Client/src/Blazor` and `Client/src/Web`. |

### Run the tests

```bash
dotnet test Api/src/usrm/tests/UnitTests/UserManagement.Core.Tests
dotnet test Api/src/usrm/tests/UnitTests/UserManagement.Infrastructure.Tests
dotnet test Api/src/usrm/tests/UnitTests/UserManagement.Client.Tests
```

The integration test projects call a running API, so start the AppHost first.

### Run the MAUI app (optional)

```bash
cd Client/src/Maui
dotnet build -t:Run -f net9.0-windows10.0.19041.0     # Windows
dotnet build -t:Run -f net9.0-android                 # Android emulator or device
```

A device or emulator reaches the APIs on your machine through its own address rather than `localhost`; see [`Client/src/Maui/README.md`](./Client/src/Maui/README.md).

## API Endpoints

### Alert Management (`/api`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/alerts` | Get all alerts |
| GET | `/alerts/active` | Get current active alert |
| GET | `/alerts/{id}` | Get alert by ID |
| GET | `/alerts/{id}/history` | Get alert history |
| POST | `/alerts` | Create new alert |
| PUT | `/alerts/{id}/status` | Update alert status |
| DELETE | `/alerts/{id}/rescind` | Rescind alert |

### Notifications (`/api`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/notifications/register` | Register device for push |
| DELETE | `/notifications/unregister/{id}` | Unregister device |
| POST | `/notifications/send/all` | Send to all devices |
| POST | `/notifications/send/user/{userId}` | Send to specific user |
| POST | `/notifications/send/tags` | Send to tagged devices |
| POST | `/notifications/test` | Test notification |

### User Management (`/api`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Register new user |
| POST | `/auth/login` | User login |
| POST | `/auth/refresh` | Refresh token |
| GET | `/users` | Get all users |
| GET | `/users/{id}` | Get user by ID |
| PUT | `/users/{id}` | Update user |
| DELETE | `/users/{id}` | Delete user |

### User Settings (`/api`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users/{userId}/settings` | Get user settings |
| PUT | `/users/{userId}/settings` | Update user settings |
| POST | `/users/{userId}/settings/default` | Create default settings |

### Invitations (`/api`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/invitations` | Get all invitations |
| GET | `/invitations/{id}` | Get invitation by ID |
| POST | `/invitations` | Create new invitation |
| POST | `/invitations/accept` | Accept invitation |
| POST | `/invitations/{id}/resend` | Resend invitation |
| DELETE | `/invitations/{id}` | Delete invitation |
| GET | `/invitations/validate/{code}` | Validate activation code |

## Configuration

### Azure App Configuration Keys

| Key | Description |
|-----|-------------|
| `AzureNotificationHubs:ConnectionString` | Notification Hub connection (Key Vault reference) |
| `AzureNotificationHubs:HubName` | Notification Hub name |

### Azure resources (deployed environments only)

Local development needs none of these (see [Getting Started](#getting-started)). A deployed environment, provisioned by [`IaC/bicep/main.bicep`](./IaC/README.md), uses:

- Azure Cosmos DB
- Azure Notification Hub (with FCM/APNs configured)
- Azure App Configuration
- Azure Key Vault
- Azure Communication Services (for email)
- Azure Function Apps and a Static Web App

## Testing

### Run Unit Tests
See [Run the tests](#run-the-tests) under Getting Started.

### Run Integration Tests
Start the AppHost, then:
```bash
dotnet test Api/tests/IntegrationTests/AlertManagement.Client.IntegrationTests
dotnet test Api/src/usrm/tests/IntegrationTests/UserManagement.Client.IntegrationTests
```

### HTTP Test Files
REST Client test files are available in:
- `Api/tests/IntegrationTests/AlertManagement.Client.IntegrationTests/httpFiles/`
- `Api/src/usrm/tests/IntegrationTests/UserManagement.Client.IntegrationTests/httpFiles/`

## Deployment

### GitHub Actions Workflows

- `deploy-infrastructure.yml` - Deploy the Bicep infrastructure (runs on every push to `master`)
- `deploy-alertmanagement-api.yml` - Deploy Alert Management API (manual)
- `deploy-usermanagement-api.yml` - Deploy User Management API (manual)
- `deploy-android-playstore.yml` - Deploy Android app to Play Store (version tags)
- `deploy-windows-blob.yml` - Build and publish the Windows MSI (version tags)
- `azure-static-web-apps-*.yml` - Web app preview environments for pull requests

## Project Board

Track development progress: [AlertOne Development](https://github.com/users/firemanwayne/projects/1)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is proprietary software. All rights reserved.

## Support

For issues and feature requests, please use the [GitHub Issues](https://github.com/firemanwayne/AlertOne/issues) page.
