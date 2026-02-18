# Coding Standards

This document outlines the coding standards and conventions used in the UserManagement project.

## General Principles

### 1. Always Use Braces for Control Flow Statements

**Rule**: All control flow statements (if, else, for, foreach, while, do-while, etc.) must have braces, even for single-line statements.

**Rationale**:
- Prevents bugs when adding additional statements
- Improves code readability and consistency
- Reduces risk of errors during refactoring
- Follows Microsoft's C# coding conventions
- Makes code reviews easier

**Before (Bad)**:
```csharp
// DON'T DO THIS
if (user is null)
    return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

// Or this
if (count > 0) DoSomething();
```

**After (Good)**:
```csharp
// DO THIS INSTEAD
if (user is null)
{
    return Result.Failure(Error.NotFound("User.NotFound", "User not found"));
}

// And this
if (count > 0)
{
    DoSomething();
}
```

**Applies to all control flow**:
```csharp
// If statements
if (condition)
{
    statement;
}

// Else statements
if (condition)
{
    statement1;
}
else
{
    statement2;
}

// For loops
for (int i = 0; i < count; i++)
{
    statement;
}

// Foreach loops
foreach (var item in collection)
{
    statement;
}

// While loops
while (condition)
{
    statement;
}

// Do-while loops
do
{
    statement;
}
while (condition);
```

### 2. No Tuples - Use Response Objects

**Rule**: Do not use tuples for method return values. Instead, create proper response objects to encapsulate data.

**Rationale**:
- Response objects provide better type safety and documentation
- They are more maintainable and easier to extend
- They make the code more self-documenting and easier to understand
- They follow the principle: "Requests go in, Responses come out"

**Before (Bad)**:
```csharp
// DON'T DO THIS
public Task<Result<(string RefreshToken, DeviceSessionResponse Device)>> RegisterOrUpdateDeviceAsync(
    UserId userId,
    DeviceInfo deviceInfo,
    string? ipAddress,
    CancellationToken cancellationToken = default);

// Usage
var result = await service.RegisterOrUpdateDeviceAsync(userId, deviceInfo, ipAddress);
var (refreshToken, device) = result.Value; // Tuple deconstruction
```

**After (Good)**:
```csharp
// DO THIS INSTEAD
public Task<Result<RegisterDeviceResponse>> RegisterOrUpdateDeviceAsync(
    UserId userId,
    DeviceInfo deviceInfo,
    string? ipAddress,
    CancellationToken cancellationToken = default);

// Response object
public sealed record RegisterDeviceResponse(
    string RefreshToken,
    DeviceSessionResponse Device);

// Usage
var result = await service.RegisterOrUpdateDeviceAsync(userId, deviceInfo, ipAddress);
var refreshToken = result.Value.RefreshToken;
var device = result.Value.Device;
```

**Benefits**:
- Clear, named properties instead of positional tuple elements
- Easy to add new properties without breaking existing code
- Better IntelliSense and tooling support
- Self-documenting through property names

### 3. Requests Go In, Responses Come Out

**Rule**: All public service methods should follow the pattern of accepting Request objects as input and returning Response objects as output.

**Structure**:
- **Requests**: Located in `Core/DTOs/Requests/`
- **Responses**: Located in `Core/DTOs/Responses/`

**Example**:
```csharp
// Request DTO
public sealed record LoginRequest(
    string Email,
    string Password,
    DeviceInfo Device);

// Response DTO
public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User,
    IEnumerable<UserClaim> Claims,
    DeviceSessionResponse CurrentDevice);

// Service method
public Task<Result<AuthenticationResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default);
```

### 4. Use Records for DTOs

**Rule**: Use C# records for Data Transfer Objects (DTOs) including Requests and Responses.

**Rationale**:
- Records provide value-based equality
- Immutable by default
- Concise syntax with primary constructors
- Built-in ToString(), GetHashCode(), and Equals() implementations

**Example**:
```csharp
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);
```

### 5. Use Primary Constructors

**Rule**: Use C# 12 primary constructors for services and classes where dependency injection is used.

**Example**:
```csharp
// Good - Primary constructor
public sealed class DeviceService(
    ApplicationDbContext context,
    ILogger<DeviceService> logger) : IDeviceService
{
    // Fields are automatically created from parameters
}

// Instead of:
public sealed class DeviceService : IDeviceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(ApplicationDbContext context, ILogger<DeviceService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

### 6. Value Objects for Domain Concepts

**Rule**: Use value objects (records) for domain concepts that have validation logic or specific meaning.

**Examples**:
- `Email` - Validates email format
- `UserId` - Type-safe wrapper around Guid
- `UserClaim` - Encapsulates claim type and value

**Example**:
```csharp
public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(
                Error.Validation("Email.Empty", "Email cannot be empty"));
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return Result.Failure<Email>(
                Error.Validation("Email.Invalid", "Email format is invalid"));
        }

        return Result.Success(new Email(email.ToLowerInvariant()));
    }
}
```

### 7. Result Pattern for Error Handling

**Rule**: Use the Result pattern for operations that can fail instead of throwing exceptions.

**Example**:
```csharp
public async Task<Result<AuthenticationResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
{
    var user = await userManager.FindByEmailAsync(request.Email);

    if (user is null)
    {
        return Result.Failure<AuthenticationResponse>(
            Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password"));
    }

    // ... success path
    return Result.Success(response);
}
```

### 8. Sealed Classes by Default

**Rule**: Mark classes as `sealed` unless they are explicitly designed for inheritance.

**Rationale**:
- Improves performance (JIT optimizations)
- Makes intent clear
- Prevents unintended inheritance

### 9. Enum Naming

**Rule**: Use descriptive enum names and provide an `Unknown` or default value as the first enum member.

**Example**:
```csharp
public enum DeviceType
{
    Unknown = 0,  // Always provide a default value
    Web = 1,
    Mobile = 2,
    Desktop = 3
}
```

## Project Structure

- **Core Layer**: Pure business logic, no external dependencies
  - Entities, Value Objects, Interfaces, DTOs, Common (Result, Error)

- **Infrastructure Layer**: Implementation details
  - Services, Database Context, Identity, EF Migrations

- **Client Layer**: Azure Functions, API endpoints
  - HTTP Triggers, Middleware, Extensions

## Naming Conventions

- **Interfaces**: Prefix with `I` (e.g., `IUserService`)
- **DTOs**: Suffix with Request/Response (e.g., `LoginRequest`, `UserResponse`)
- **Private fields**: Use `_camelCase` when needed (primary constructors eliminate most private fields)
- **Methods**: Use `PascalCase` and async methods should end with `Async`
- **Constants**: Use `PascalCase` for public constants, `UPPER_CASE` for private constants

## Documentation

- **Public APIs**: Always include XML documentation comments
- **Complex Logic**: Add inline comments explaining "why", not "what"
- **DTOs**: Document each property's purpose

## Testing

- **Test Method Names**: Use descriptive names following pattern: `MethodName_Scenario_ExpectedBehavior`
- **Arrange-Act-Assert**: Structure all tests with clear sections
- **Test Data**: Use helper methods to create test data (e.g., `CreateTestDeviceInfo()`)

## Summary

Following these coding standards ensures:
- ✅ Consistent codebase
- ✅ Better maintainability
- ✅ Improved type safety
- ✅ Self-documenting code
- ✅ Easier onboarding for new developers
