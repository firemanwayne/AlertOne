# HTTP Files for API Testing

This folder contains `.http` files for testing the UserManagement API endpoints using REST Client extensions in VS Code or Visual Studio.

## Setup

### Visual Studio Code
1. Install the **REST Client** extension by Huachao Mao
2. Open any `.http` file
3. Click "Send Request" above each request

### Visual Studio 2022
1. Built-in support for `.http` files (no extension needed)
2. Open any `.http` file
3. Click the green "Run" button next to each request

### JetBrains Rider
1. Built-in support for `.http` files
2. Open any `.http` file
3. Click the "Run" icon next to each request

## Files

### `auth.http`
Tests authentication endpoints:
- User registration (with validation tests)
- User login (with error cases)
- Token refresh
- Various edge cases and error scenarios

### `users.http`
Tests user management endpoints:
- Get user by ID
- Get all users
- Update user profile
- Delete user
- Complete user workflow test

### `roles.http`
Tests role management endpoints:
- Create, read, update, delete roles
- Assign/remove roles to/from users
- Get users in specific role
- Complete role management workflow

## Usage

### 1. Start the Azure Functions Locally

```bash
cd src/UserManagement.Client
func start
```

Or using dotnet:
```bash
dotnet run --project src/UserManagement.Client
```

The API should be running at `http://localhost:7071`

### 2. Apply Database Migrations

```bash
cd src/UserManagement.Infrastructure
dotnet ef database update --startup-project ../UserManagement.Client
```

### 3. Test Authentication Flow

1. Open `auth.http`
2. Send the **Register New User** request (#1)
3. Copy the `accessToken` and `refreshToken` from the response
4. Send the **Login** request (#6) to test login
5. Use the tokens in subsequent requests

### 4. Test User Management

1. Open `users.http`
2. Update the `@accessToken` and `@userId` variables at the top with values from authentication
3. Send requests to test user operations

### 5. Test Role Management

1. Open `roles.http`
2. Update the `@accessToken`, `@roleId`, and `@userId` variables
3. Send requests to test role operations

## Variables

Each `.http` file uses variables for reusability:

```http
@baseUrl = http://localhost:7071/api
@contentType = application/json
@accessToken = YOUR_TOKEN_HERE
@userId = YOUR_USER_ID_HERE
@roleId = YOUR_ROLE_ID_HERE
```

**Important:** You need to manually copy and paste tokens and IDs from responses into these variables for subsequent requests.

## Tips

### Sequential Testing
Follow the numbered requests in order for the most logical test flow.

### Workflow Tests
Each file includes a complete workflow test at the end demonstrating a full CRUD cycle.

### Error Testing
Requests marked with "Should fail" are intentionally designed to test error handling.

### Response Inspection
- **Status Codes:** Check that responses return expected HTTP status codes
- **Response Body:** Verify the JSON structure matches expected DTOs
- **Error Messages:** Ensure error responses contain meaningful codes and messages

## Expected Status Codes

### Success Codes
- `200 OK` - Successful GET, PUT requests
- `201 Created` - Successful POST (register, create role)
- `204 No Content` - Successful DELETE

### Error Codes
- `400 Bad Request` - Validation errors (invalid email, weak password, etc.)
- `401 Unauthorized` - Authentication failed (wrong credentials, missing token)
- `403 Forbidden` - Account locked out
- `404 Not Found` - Resource not found (user, role)
- `409 Conflict` - Duplicate resource (email exists, role name taken)
- `500 Internal Server Error` - Unexpected server error

## Example Test Sequence

### Complete User Lifecycle
```
1. Register User → 201 Created
2. Login → 200 OK (get tokens)
3. Get User Details → 200 OK
4. Update User → 200 OK
5. Get User Details (verify) → 200 OK
6. Delete User → 204 No Content
7. Get User Details (verify deletion) → 404 Not Found
```

### Complete Role Lifecycle
```
1. Create Role → 201 Created
2. Get Role by Name → 200 OK
3. Assign to User → 200 OK
4. Get Users in Role → 200 OK (verify user in list)
5. Update Role Name → 200 OK
6. Remove from User → 200 OK
7. Delete Role → 204 No Content
8. Get Role (verify deletion) → 404 Not Found
```

## Seeded Data

The database is seeded with two roles on creation:
- **Admin** role (ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890)
- **User** role (ID: b2c3d4e5-f6a7-8901-bcde-f12345678901)

New users are automatically assigned the **User** role upon registration.

## Troubleshooting

### "Connection refused" error
- Ensure Azure Functions is running (`func start`)
- Verify the port is 7071 or update `@baseUrl`

### "Unauthorized" errors
- Ensure you've logged in and copied the access token
- Tokens expire after 1 hour - login again if expired

### "User not found" errors
- Verify the userId in the `@userId` variable is correct
- User may have been deleted in a previous test

### Database errors
- Ensure migrations are applied
- Check SQLite database file exists: `src/UserManagement.Client/usermanagement.db`

## Automation

For automated testing, consider:
- Converting these to actual integration tests (MSTest)
- Using Postman collections
- Creating automated test scripts with curl or PowerShell

## Notes

- These files are for **manual testing** and **exploration** during development
- For **automated testing**, see the integration test projects in the solution
- Always test against a **development database**, not production
- Some tests are **destructive** (delete operations) - be careful with test data
