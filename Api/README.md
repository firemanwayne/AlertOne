# User Management Azure Functions Application

A production-ready Azure Functions application for user management using ASP.NET Core Identity, built with clean architecture principles.

## 📚 Documentation

- **[QuickStart Guide](./QUICKSTART.md)** - Get running in 5 minutes
- **[Local Development](./LOCAL_DEVELOPMENT.md)** - Complete local development setup guide
- **[MAUI Connection Guide](./MAUI_CONNECTION_GUIDE.md)** - Connect .NET MAUI apps to the local API
- **[Notification Integration](./NOTIFICATION_INTEGRATION.md)** - Push notification support for external services
- **[Coding Standards](./CODING_STANDARDS.md)** - Development guidelines and best practices
- **[Coding Standards Analysis](./CODING_STANDARDS_ANALYSIS.md)** - Code quality report

## Architecture

This solution follows **Clean Architecture** with three distinct layers:

- **Core** (`UserManagement.Core`): Domain layer with business logic, value objects, and interfaces (no external dependencies)
- **Infrastructure** (`UserManagement.Infrastructure`): Implementation layer with EF Core, ASP.NET Core Identity, and JWT token services
- **Client** (`UserManagement.Client`): Azure Functions host with HTTP triggers for API endpoints

## Technology Stack

- **.NET 8** - Latest LTS version with isolated worker model
- **Azure Functions v4** - Serverless compute platform
- **ASP.NET Core Identity** - User management and authentication
- **Entity Framework Core 8** - Data access with migrations
- **JWT Bearer Tokens** - Stateless authentication
- **SQLite** (development) / **SQL Server** (production)

## Project Structure

```
UserManagement/
├── src/
│   ├── UserManagement.Core/           # Domain layer
│   │   ├── Common/                    # Result pattern, Error types
│   │   ├── Entities/                  # Domain entities
│   │   ├── ValueObjects/              # Email, UserId, UserRole
│   │   ├── Interfaces/                # Service contracts
│   │   └── DTOs/                      # Request/Response models
│   ├── UserManagement.Infrastructure/ # Implementation layer
│   │   ├── Identity/                  # ApplicationUser, DbContext
│   │   ├── Services/                  # Service implementations
│   │   ├── Configurations/            # Settings models
│   │   └── Extensions/                # DI configuration
│   └── UserManagement.Client/         # Azure Functions
│       ├── Functions/                 # HTTP triggers
│       ├── Middleware/                # Exception handling, Auth
│       └── Extensions/                # HTTP helpers
└── tests/
    ├── UnitTests/                     # Unit test projects
    └── IntegrationTests/              # Integration test projects
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/refresh` - Refresh access token

### User Management (Authenticated)
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user profile
- `DELETE /api/users/{id}` - Delete user
- `GET /api/users` - Get all users

### Role Management (Admin)
- `POST /api/roles` - Create a new role
- `GET /api/roles` - Get all roles
- `GET /api/roles/{id}` - Get role by ID
- `GET /api/roles/name/{name}` - Get role by name
- `PUT /api/roles/{id}` - Update role name
- `DELETE /api/roles/{id}` - Delete role (only if no users assigned)
- `POST /api/roles/assign` - Assign role to user
- `POST /api/roles/remove` - Remove role from user
- `GET /api/roles/{name}/users` - Get all users in a specific role

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure Storage Emulator](https://docs.microsoft.com/azure/storage/common/storage-use-emulator) or Azurite

### Setup

1. **Clone the repository and navigate to the project folder**

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Apply database migrations**
   ```bash
   cd src/UserManagement.Infrastructure
   dotnet ef database update --startup-project ../UserManagement.Client
   ```

5. **Run the Azure Functions locally**
   ```bash
   cd ../UserManagement.Client
   func start --port 7071
   ```

   Or with .NET CLI:
   ```bash
   dotnet run --project src/UserManagement.Client
   ```

The API will be available at:
- **HTTP**: `http://localhost:7071/api`

📖 **See [QUICKSTART.md](./QUICKSTART.md) for 5-minute setup guide**
📖 **See [LOCAL_DEVELOPMENT.md](./LOCAL_DEVELOPMENT.md) for full local development guide**

## Configuration

### Local Development (`local.settings.json`)

The `local.settings.json` file contains development settings:

```json
{
  "Values": {
    "DatabaseSettings__Provider": "Sqlite",
    "DatabaseSettings__ConnectionString": "Data Source=usermanagement.db",
    "JwtSettings__Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForDevelopment!",
    "JwtSettings__Issuer": "UserManagement.API",
    "JwtSettings__Audience": "UserManagement.Client",
    "JwtSettings__ExpiryMinutes": "60"
  }
}
```

### Production Configuration

For production, configure the following in Azure Function App settings:

- `DatabaseSettings__Provider`: "SqlServer"
- `DatabaseSettings__ConnectionString`: Your Azure SQL connection string
- `JwtSettings__Secret`: Secure secret key (min 256 bits) from Azure Key Vault
- `JwtSettings__Issuer`: Your issuer URL
- `JwtSettings__Audience`: Your audience URL

## Testing the API

### Automated Test Scripts

Run the included test scripts to verify all endpoints:

**PowerShell** (Windows):
```powershell
.\test-api.ps1
```

**Bash** (Linux/macOS):
```bash
chmod +x test-api.sh
./test-api.sh
```

### Manual Testing Examples

### 1. Register a New User

```bash
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "device": {
      "deviceId": "my-device-123",
      "deviceName": "My Device",
      "deviceType": 1,
      "platform": "Windows",
      "appVersion": "1.0.0",
      "userAgent": "curl"
    }
  }'
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken...",
  "expiresAt": "2026-02-10T12:00:00Z",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"],
    "createdAt": "2026-02-10T11:00:00Z",
    "lastModifiedAt": null
  }
}
```

### 2. Login

```bash
curl -X POST http://localhost:7071/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "device": {
      "deviceId": "my-device-123",
      "deviceName": "My Device",
      "deviceType": 1,
      "platform": "Windows"
    }
  }'
```

### 3. Get User by ID (Authenticated)

```bash
curl -X GET http://localhost:7071/api/users/{userId} \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. Update User

```bash
curl -X PUT http://localhost:7071/api/users/{userId} \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith"
  }'
```

### 5. Create a Role

```bash
curl -X POST http://localhost:7071/api/roles \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Manager"
  }'
```

### 6. Get All Roles

```bash
curl -X GET http://localhost:7071/api/roles \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 7. Assign Role to User

```bash
curl -X POST http://localhost:7071/api/roles/assign \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-guid-here",
    "roleName": "Manager"
  }'
```

### 8. Get Users in Role

```bash
curl -X GET http://localhost:7071/api/roles/Manager/users \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Key Features

### Clean Architecture
- **Domain-Driven Design**: Core business logic isolated from infrastructure
- **Dependency Inversion**: All dependencies point inward to the Core
- **Testability**: Each layer can be tested independently

### Security
- **Password Hashing**: PBKDF2 via ASP.NET Core Identity
- **JWT Tokens**: Stateless authentication with 1-hour expiration
- **Refresh Tokens**: 7-day validity for token renewal
- **Account Lockout**: 5 failed attempts = 15-minute lockout
- **Password Policy**: Min 8 characters, requires digit, upper, lower, special char

### Error Handling
- **Result Pattern**: Explicit success/failure with typed errors
- **Global Exception Middleware**: Catches unhandled exceptions
- **Consistent Error Responses**: Standard error format across all endpoints

### Database
- **Code-First Migrations**: EF Core migrations for schema management
- **Provider Flexibility**: SQLite for dev, SQL Server for production
- **Identity Integration**: Customized table names and seeded roles

## Database Schema

The application creates the following tables:

- `Users` - User accounts with profile information
- `Roles` - User roles (Admin, User)
- `UserRoles` - Many-to-many relationship
- `UserClaims`, `UserLogins`, `UserTokens` - Identity framework tables
- `RoleClaims` - Role-based claims

### Seeded Roles

- **Admin** - Administrative access
- **User** - Standard user access

## Development Workflow

### Creating New Migrations

```bash
cd src/UserManagement.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../UserManagement.Client
dotnet ef database update --startup-project ../UserManagement.Client
```

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish src/UserManagement.Client -c Release -o ./publish
```

## Deployment to Azure

1. **Create Azure Resources**
   - Azure Function App (Consumption or Premium plan)
   - Azure SQL Database
   - Azure Key Vault (for secrets)
   - Application Insights (for monitoring)

2. **Configure App Settings**
   - Set `DatabaseSettings__ConnectionString` to Azure SQL
   - Set `DatabaseSettings__Provider` to "SqlServer"
   - Configure JWT settings from Key Vault

3. **Deploy Function App**
   ```bash
   func azure functionapp publish <FunctionAppName>
   ```

4. **Apply Migrations**
   ```bash
   dotnet ef database update --connection "YourAzureSqlConnectionString"
   ```

## Next Steps

### Recommended Enhancements

1. **Email Confirmation** - Add email verification flow with SendGrid
2. **Password Reset** - Implement forgot password functionality
3. **Two-Factor Authentication** - Add 2FA support
4. **Admin Endpoints** - Create role-based admin functions
5. **API Documentation** - Add Swagger/OpenAPI documentation
6. **Rate Limiting** - Implement API throttling
7. **Unit Tests** - Add comprehensive test coverage
8. **Integration Tests** - Test end-to-end scenarios
9. **Logging** - Enhanced structured logging with Application Insights
10. **CI/CD Pipeline** - Automated deployment with GitHub Actions or Azure DevOps

### Testing Improvements

The test projects are scaffolded and ready for implementation:

- **Core.Tests**: Value object validation, domain logic
- **Infrastructure.Tests**: Service implementations with mocking
- **Client.Tests**: Function HTTP responses
- **IntegrationTests**: End-to-end API testing

## Troubleshooting

### Common Issues

**Issue**: "Table already exists" error during migration
- **Solution**: Drop the database or remove existing migrations

**Issue**: JWT token validation fails
- **Solution**: Ensure the JWT secret is at least 256 bits (32 characters)

**Issue**: Cannot connect to database
- **Solution**: Check connection string in `local.settings.json`

**Issue**: Azure Functions not starting
- **Solution**: Ensure Azure Storage Emulator or Azurite is running

## License

This project is provided as-is for educational and commercial use.

## Support

For issues and questions:
1. Check the Azure Functions documentation
2. Review the ASP.NET Core Identity docs
3. Check EF Core migration guides

---

**Built with Claude Code** - Production-ready Azure Functions with Clean Architecture
