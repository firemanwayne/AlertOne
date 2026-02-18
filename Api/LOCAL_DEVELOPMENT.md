# Local Development Guide

## Running the Azure Function Locally

### Prerequisites

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Azure Functions Core Tools v4** - [Installation Guide](https://docs.microsoft.com/azure/azure-functions/functions-run-local)

### Run the Function

Navigate to the Client project directory:
```bash
cd src/UserManagement.Client
```

#### Option A: Using Azure Functions Core Tools (Recommended)

```bash
func start --port 7071
```

#### Option B: Using .NET CLI

```bash
dotnet run
```

#### Option C: Using Visual Studio

1. Open the solution in Visual Studio
2. Set `UserManagement.Client` as the startup project
3. Press F5 to start debugging

### Access the API

Once the function is running, you'll see output like:

```
Functions:

        AuthFunctions:

                [POST] http://localhost:7071/api/auth/register

                [POST] http://localhost:7071/api/auth/login

                [POST] http://localhost:7071/api/auth/refresh

        UserFunctions:

                [GET] http://localhost:7071/api/users

                [GET] http://localhost:7071/api/users/{id}

                [POST] http://localhost:7071/api/users

                [PUT] http://localhost:7071/api/users/{id}

                [DELETE] http://localhost:7071/api/users/{id}

        DeviceFunctions:

                [GET] http://localhost:7071/api/devices

                [GET] http://localhost:7071/api/devices/{id}

                [DELETE] http://localhost:7071/api/devices/{id}

                [DELETE] http://localhost:7071/api/devices/all

                [DELETE] http://localhost:7071/api/devices/all-except/{id}

                [PUT] http://localhost:7071/api/devices/{id}/trust

                [PUT] http://localhost:7071/api/devices/{id}/name

                [GET] http://localhost:7071/api/users/{userId}/devices/notifications
```

**Base URL**: `http://localhost:7071`

### Testing the API

#### Using curl

```bash
# Register a new user
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "device": {
      "deviceId": "test-device-123",
      "deviceName": "Test Device",
      "deviceType": 1,
      "platform": "Windows",
      "appVersion": "1.0.0",
      "userAgent": "curl",
      "pushNotificationToken": null
    }
  }'
```

#### Using PowerShell

```powershell
# Register a new user
$body = @{
    email = "test@example.com"
    password = "Password123!"
    confirmPassword = "Password123!"
    firstName = "John"
    lastName = "Doe"
    device = @{
        deviceId = "test-device-123"
        deviceName = "Test Device"
        deviceType = 1
        platform = "Windows"
        appVersion = "1.0.0"
        userAgent = "PowerShell"
        pushNotificationToken = $null
    }
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:7071/api/auth/register" `
                               -Method Post `
                               -Body $body `
                               -ContentType "application/json"

$response
```

#### Using Postman

1. **Import the collection** (optional):
   - Create a new collection for the API
   - Set base URL variable: `{{baseUrl}} = http://localhost:7071`

2. **Test endpoints**:
   - Register: `POST {{baseUrl}}/api/auth/register`
   - Login: `POST {{baseUrl}}/api/auth/login`
   - Get Users: `GET {{baseUrl}}/api/users`

### Database

The local configuration uses SQLite for the database.

**Database File**: `src/UserManagement.Client/usermanagement.db`

#### Apply Migrations

Migrations are applied automatically on startup. If you need to apply them manually:

```bash
cd src/UserManagement.Infrastructure

# Create a new migration
dotnet ef migrations add MigrationName --startup-project ../UserManagement.Client

# Update database
dotnet ef database update --startup-project ../UserManagement.Client

# View migration SQL
dotnet ef migrations script --startup-project ../UserManagement.Client
```

#### View Database

**Using SQLite Browser**:
1. Download [DB Browser for SQLite](https://sqlitebrowser.org/)
2. Open `src/UserManagement.Client/usermanagement.db`
3. Browse data in the "Browse Data" tab

**Using Command Line**:
```bash
cd src/UserManagement.Client
sqlite3 usermanagement.db

# View tables
.tables

# Query users
SELECT * FROM Users;

# Exit
.quit
```

### Configuration

Local settings are configured in `local.settings.json`:

```json
{
  "Values": {
    "DatabaseSettings__Provider": "Sqlite",
    "DatabaseSettings__ConnectionString": "Data Source=usermanagement.db",
    "JwtSettings__Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForDevelopment!",
    "JwtSettings__Issuer": "UserManagement.API",
    "JwtSettings__Audience": "UserManagement.Client",
    "JwtSettings__ExpiryMinutes": "60"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

**Note**: `local.settings.json` is not committed to source control (.gitignore). Copy `local.settings.example.json` if it exists.

### CORS Configuration

The local configuration enables CORS for all origins (`"*"`). This allows requests from:
- Local web apps (React, Angular, Vue, etc.)
- Postman
- curl
- Any other HTTP client

**To restrict CORS in production**, update the CORS settings in Azure or modify the middleware.

### Ports

| Service | Protocol | Port | URL |
|---------|----------|------|-----|
| Azure Function | HTTP | 7071 | http://localhost:7071 |

### Troubleshooting

#### Port Already in Use

**Problem**: "Address already in use" or "Failed to bind to address"

**Solution**:
```bash
# Find process using port 7071 (Windows)
netstat -ano | findstr :7071

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# On Linux/macOS
lsof -i :7071
kill -9 <PID>
```

Or use a different port:
```bash
func start --port 7073
```

#### Function Not Starting

**Problem**: Azure Functions Core Tools not found

**Solution**:
```bash
# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4
```

**Problem**: "DatabaseSettings not configured"

**Solution**: Ensure `local.settings.json` exists with proper configuration.

#### Migration Issues

**Problem**: "No DbContext was found"

**Solution**:
```bash
cd src/UserManagement.Infrastructure
dotnet ef database update --startup-project ../UserManagement.Client
```

**Problem**: "A file or folder with the name 'usermanagement.db' already exists"

**Solution**: Delete the existing database file and run migrations again:
```bash
cd src/UserManagement.Client
rm usermanagement.db
dotnet run
```

### IDE Configuration

#### Visual Studio 2022

1. **Set launch profile**:
   - Right-click project → Properties → Debug
   - Select "UserManagement.Client" profile

2. **Configure debugging**:
   - Set breakpoints in your code
   - Press F5 to start debugging
   - Function will start on http://localhost:7071

#### Visual Studio Code

1. **Install extensions**:
   - Azure Functions
   - C# Dev Kit

2. **Configure launch.json**:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Attach to .NET Functions",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:azureFunctions.pickProcess}"
    }
  ]
}
```

3. **Start debugging**:
   - Press F1 → "Azure Functions: Start Function App"
   - Or use terminal: `func start`

#### Rider

1. **Create run configuration**:
   - Run → Edit Configurations
   - Add new ".NET Project" configuration
   - Set Program arguments: `--port 7071`

2. **Start with debugging**:
   - Press Shift+F10 to run
   - Press Shift+F9 to debug

### Best Practices for Local Development

1. **Keep secrets in local.settings.json** (not in source control)
2. **Use a separate test database** for destructive operations
3. **Run tests before committing** (`dotnet test`)
4. **Clear database between tests** if needed for clean state
5. **Monitor logs** for errors and warnings
6. **Use Postman collections** for consistent API testing

### Environment Variables

Create environment-specific settings by adding configuration files:

**appsettings.Development.json** (already exists):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "DatabaseSettings": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=usermanagement.db"
  }
}
```

### Hot Reload

Azure Functions supports hot reload for code changes:

1. Start the function with `func start`
2. Make code changes
3. Save the file
4. Function will automatically reload (may take a few seconds)

**Note**: Some changes (like adding new dependencies) require a full restart.

### Next Steps

- 📖 [API Documentation](./API_DOCUMENTATION.md) - Full API reference
- 🔔 [Notification Integration](./NOTIFICATION_INTEGRATION.md) - Push notifications setup
- 📋 [Coding Standards](./CODING_STANDARDS.md) - Development guidelines
- 🧪 [Running Tests](./README.md#testing) - Unit and integration tests

---

**Need Help?**
- Check [Troubleshooting](#troubleshooting) section above
- Review Azure Functions [documentation](https://docs.microsoft.com/azure/azure-functions/)
- Open an issue in the repository
