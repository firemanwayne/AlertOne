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
│   │       └── AlertManagement.Infrastructure/  # EF Core, services
│   ├── usrm/                     # User Management API
│   │   └── src/
│   │       ├── UserManagement.Client/       # Azure Functions endpoints
│   │       ├── UserManagement.Core/         # Domain models, DTOs, interfaces
│   │       └── UserManagement.Infrastructure/   # Identity, EF Core, services
│   ├── core/                     # Shared API core library
│   ├── infrastructure/           # Shared infrastructure libraries
│   │   ├── Azure.AppConfig/      # Azure App Configuration
│   │   ├── Azure.Communications/ # Azure Communication Services (Email)
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
- **Azure SQL Database** - Relational data storage
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

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Android SDK (for MAUI development)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/firemanwayne/AlertOne.git
   cd AlertOne
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore AlertOne.Aspire.slnx
   ```

3. **Configure local settings**

   For each Azure Function app, create/update `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "AzureNotificationHubs:HubName": "your-hub-name"
     },
     "ConnectionStrings": {
       "SqlServer": "your-connection-string"
     }
   }
   ```

4. **Login to Azure** (for App Configuration and Key Vault access)
   ```bash
   az login
   ```

5. **Run the APIs**
   ```bash
   # Alert Management API
   cd Api/alerts/src/AlertManagement.Client
   func start

   # User Management API (in another terminal)
   cd Api/usrm/src/UserManagement.Client
   func start
   ```

6. **Run the Web App**
   ```bash
   cd Client/src/Web
   dotnet run
   ```

7. **Run the MAUI App**
   ```bash
   cd Client/src/Maui
   dotnet build -t:Run -f net9.0-android
   ```

### Using .NET Aspire

For orchestrated local development:
```bash
cd Aspire/AlertOne.AppHost
dotnet run
```

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

### Required Azure Resources

- Azure SQL Database
- Azure Notification Hub (with FCM/APNs configured)
- Azure App Configuration
- Azure Key Vault
- Azure Communication Services (for email)

## Testing

### Run Unit Tests
```bash
dotnet test Api/tests/UnitTests/
```

### Run Integration Tests
```bash
dotnet test Api/tests/IntegrationTests/
```

### HTTP Test Files
REST Client test files are available in:
- `Api/tests/IntegrationTests/AlertManagement.Client.IntegrationTests/httpFiles/`
- `Api/usrm/tests/IntegrationTests/UserManagement.Client.IntegrationTests/httpFiles/`

## Deployment

### GitHub Actions Workflows

- `deploy-alertmanagement-api.yml` - Deploy Alert Management API
- `deploy-usermanagement-api.yml` - Deploy User Management API
- `deploy-android-playstore.yml` - Deploy Android app to Play Store
- `azure-static-web-apps-*.yml` - Deploy web app

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
