# QuickStart Guide

Get the User Management API running locally with HTTPS in 5 minutes.

## Prerequisites

- ✅ .NET 8 SDK installed
- ✅ Azure Functions Core Tools v4 installed
- ✅ Development certificate trusted

## Step 1: Clone and Navigate

```bash
cd C:\Users\wayne.davis\source\repos\UserManagement
```

## Step 2: Restore Dependencies

```bash
dotnet restore
```

## Step 3: Build the Solution

```bash
dotnet build
```

## Step 4: Start the Function

### Option A: Using Azure Functions Core Tools

```bash
cd src/UserManagement.Client
func start --port 7071
```

### Option B: Using .NET CLI

```bash
cd src/UserManagement.Client
dotnet run
```

### Option C: Using Visual Studio

1. Open `UserManagement.sln`
2. Set `UserManagement.Client` as startup project
3. Press **F5**

## Step 5: Test the API

The function is now running at **http://localhost:7071**

### Test with curl

```bash
# Register a user
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "device": {
      "deviceId": "curl-device",
      "deviceName": "Test Device",
      "deviceType": 1,
      "platform": "Windows",
      "appVersion": "1.0.0",
      "userAgent": "curl"
    }
  }'
```

### Test with PowerShell

```powershell
$body = @{
    email = "test@example.com"
    password = "Password123!"
    confirmPassword = "Password123!"
    firstName = "John"
    lastName = "Doe"
    device = @{
        deviceId = "ps-device"
        deviceName = "PowerShell Test"
        deviceType = 1
        platform = "Windows"
        appVersion = "1.0.0"
        userAgent = "PowerShell"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/auth/register" `
                  -Method Post `
                  -Body $body `
                  -ContentType "application/json"
```

## Success! 🎉

You should see a JSON response with:
- Access token (JWT)
- Refresh token
- User information
- Device session details

## What's Next?

- 📖 Read the [API Documentation](./API_DOCUMENTATION.md)
- 🔐 Learn about [Authentication](./NOTIFICATION_INTEGRATION.md)
- 💻 Review [Local Development Guide](./LOCAL_DEVELOPMENT.md)
- 🧪 Run tests: `dotnet test`

## Troubleshooting

### Port Already in Use

```bash
func start --port 7073
```

### Database Issues

```bash
cd src/UserManagement.Client
rm usermanagement.db
dotnet run
```

## Key Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/register` | POST | Register new user |
| `/api/auth/login` | POST | Login user |
| `/api/auth/refresh` | POST | Refresh access token |
| `/api/users` | GET | Get all users |
| `/api/users/{id}` | GET | Get user by ID |
| `/api/devices` | GET | Get user's devices |

## Default Configuration

- **Base URL**: `http://localhost:7071`
- **Database**: SQLite (`usermanagement.db`)
- **JWT Expiry**: 60 minutes
- **Refresh Token Expiry**: 7 days
- **CORS**: Enabled for all origins (local only)

---

**Full documentation**: [LOCAL_DEVELOPMENT.md](./LOCAL_DEVELOPMENT.md)
