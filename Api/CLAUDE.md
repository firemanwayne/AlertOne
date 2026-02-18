# Coding Standards for UserManagement Project

This document defines the coding standards and best practices that must be followed when working on the UserManagement Azure Functions application.

## Table of Contents
1. [Null Safety](#null-safety)
2. [Error Handling](#error-handling)
3. [Architecture Patterns](#architecture-patterns)
4. [Naming Conventions](#naming-conventions)
5. [Code Formatting](#code-formatting)
6. [Async/Await Best Practices](#asyncawait-best-practices)
7. [Logging Standards](#logging-standards)
8. [Validation Rules](#validation-rules)
9. [Testing Requirements](#testing-requirements)

---

## Null Safety

### **CRITICAL RULE: Never Override Nullable Warnings**

**❌ NEVER DO THIS:**
```csharp
// Bad: Suppressing nullable warnings with ! operator without null checks
var user = await _userManager.FindByIdAsync(userId.ToString());
return user!.Email; // NEVER suppress nullability like this

// Bad: Using ! operator on potentially null properties
public string Email { get; set; } = null!; // AVOID this pattern
```

**✅ ALWAYS DO THIS:**
```csharp
// Good: Explicit null checking
var user = await _userManager.FindByIdAsync(userId.ToString());
if (user is null)
{
    return Result.Failure<UserResponse>(
        Error.NotFound("User.NotFound", $"User with ID {userId} not found"));
}

// Now safe to use user.Email because we've proven it's not null
return user.Email;

// Good: Pattern matching for null checks
if (request is null)
{
    return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
}

// Good: Null-coalescing for defaults
var firstName = user.FirstName ?? "Unknown";
```

### Null Safety Rules

1. **Always check for null before accessing properties or methods**
   - Use `if (obj is null)` or `if (obj is not null)` pattern matching
   - Use `?.` null-conditional operator when appropriate
   - Use `??` null-coalescing operator for defaults

2. **Never use the null-forgiving operator (!) without justification**
   - **Exception**: Using `result.Error!` after checking `result.IsFailure` is acceptable because the Result pattern guarantees Error is not null when IsFailure is true
   - **Exception**: After explicit null checks that prove a value is not null in the same scope
   - If you must use `!` in other cases, add a comment explaining why the value cannot be null
   - Better yet, restructure the code to add an explicit null check

3. **Prefer explicit null checks over implicit ones**
   ```csharp
   // Good
   if (user is null)
       return Result.Failure(...);

   // Acceptable
   if (user == null)
       return Result.Failure(...);

   // Avoid
   if (!user)  // Don't use implicit bool conversion
   ```

4. **Handle nullable reference types properly in method signatures**
   ```csharp
   // Good: Explicit nullable parameter
   public Result UpdateUser(string? firstName, string? lastName)
   {
       if (firstName is not null)
       {
           // Safe to use firstName here
       }
   }
   ```

5. **Initialize nullable properties with null or default values explicitly**
   ```csharp
   // Good
   public string? RefreshToken { get; set; }
   public DateTime? RefreshTokenExpiryTime { get; set; }

   // Good: Non-nullable with initialization
   public string FirstName { get; set; } = string.Empty;
   ```

---

## Error Handling

### Use the Result Pattern Consistently

**✅ ALWAYS DO THIS:**
```csharp
// Good: Return Result type with proper error information
public async Task<Result<UserResponse>> GetUserAsync(UserId userId)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user is null)
    {
        return Result.Failure<UserResponse>(
            Error.NotFound("User.NotFound", $"User with ID {userId} not found"));
    }

    return Result.Success(MapToUserResponse(user));
}
```

**❌ NEVER DO THIS:**
```csharp
// Bad: Throwing exceptions for business logic failures
public async Task<UserResponse> GetUserAsync(UserId userId)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user is null)
    {
        throw new NotFoundException("User not found"); // Don't throw for expected failures
    }

    return MapToUserResponse(user);
}
```

### Error Handling Rules

1. **Use Result pattern for all service methods**
   - Return `Result<T>` for operations that return data
   - Return `Result` for operations that don't return data
   - Use appropriate Error types: Validation, NotFound, Conflict, Unauthorized, Forbidden, Internal

2. **Only throw exceptions for truly exceptional circumstances**
   - System failures (database connection lost, out of memory, etc.)
   - Programming errors (null reference that should never happen)
   - NOT for business logic failures (user not found, validation failure)

3. **Always log errors with context**
   ```csharp
   _logger.LogWarning("User not found: {UserId}", userId);
   _logger.LogError(ex, "Failed to update user {UserId}", userId);
   ```

4. **Provide meaningful error codes and messages**
   ```csharp
   // Good
   Error.NotFound("User.NotFound", $"User with ID {userId} not found");

   // Bad
   Error.NotFound("Error", "Not found"); // Too generic
   ```

---

## Architecture Patterns

### Layer Responsibilities

**Core Layer (Domain)**
- Pure business logic with NO external dependencies
- Contains: Entities, Value Objects, Interfaces, DTOs, Result pattern
- No references to: EF Core, ASP.NET Core, Azure Functions

**Infrastructure Layer**
- Implements Core interfaces
- Contains: EF Core entities, DbContext, Service implementations
- References: Core, EF Core, ASP.NET Core Identity

**Client Layer (Azure Functions)**
- HTTP triggers and API endpoints
- Thin layer that delegates to services
- References: Core, Infrastructure

### Dependency Rules

1. **Dependencies flow inward: Client → Infrastructure → Core**
2. **Core project has ZERO external dependencies**
3. **Never reference upper layers from lower layers**

### Class Access Modifiers

**CRITICAL RULE: All Implementation Classes Must Be `internal sealed`**

**✅ ALWAYS DO THIS:**
```csharp
// Good: Service implementation marked internal sealed
internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserService> logger) : IUserService
{
    // implementation
}

// Good: Infrastructure implementation marked internal sealed
internal sealed class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    // implementation
}

// Good: Value object implementation marked internal sealed
internal sealed class EmailValidator
{
    // implementation
}
```

**❌ NEVER DO THIS:**
```csharp
// Bad: Public implementation class
public class UserService : IUserService  // Should be internal sealed
{
    // implementation
}

// Bad: Not sealed
internal class AuthenticationService : IAuthenticationService  // Should be sealed
{
    // implementation
}

// Bad: Both missing
public class RoleService : IRoleService  // Should be internal sealed
{
    // implementation
}
```

### Access Modifier Rules

1. **All implementation classes must be `internal sealed`**
   - Service implementations
   - Repository implementations
   - Helper classes
   - Infrastructure components
   - Value object implementations (when not using records)

2. **Exceptions where `public` is allowed**
   - Interfaces (`public interface IUserService`)
   - DTOs and records (`public record UserResponse`)
   - Entities in Core layer (`public class User`)
   - Azure Functions classes (`public sealed class UserFunctions`)
   - Extension methods classes (`public static class HttpRequestExtensions`)

3. **Why `sealed`?**
   - **Performance**: Enables compiler optimizations (devirtualization)
   - **Intent**: Clearly signals class is not designed for inheritance
   - **Safety**: Prevents unexpected inheritance bugs
   - **Immutability**: Helps maintain predictable behavior

4. **Why `internal`?**
   - **Encapsulation**: Implementation details hidden from consumers
   - **Dependency Injection**: Classes accessed via interfaces, not concrete types
   - **Flexibility**: Can refactor internals without breaking external code
   - **Security**: Reduces attack surface by limiting visibility

### Examples by Layer

**Core Layer:**
```csharp
// Public: Domain entities
public class User
{
    // Domain logic
}

// Public: Interfaces
public interface IUserService
{
    // Contract
}

// Public: DTOs
public record UserResponse(Guid Id, string Email, string FirstName, string LastName);

// Public: Value objects
public sealed class Email
{
    // Value object logic
}
```

**Infrastructure Layer:**
```csharp
// Internal sealed: Service implementations
internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserService> logger) : IUserService
{
    // Implementation
}

internal sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    // Implementation
}

internal sealed class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    // Implementation
}

// Internal sealed: DbContext
internal sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // Configuration
}

// Internal sealed: Helper classes
internal sealed class PasswordHasher
{
    // Helper logic
}
```

**Client Layer:**
```csharp
// Public sealed: Azure Functions (must be public for Functions runtime)
public sealed class UserFunctions(
    IUserService userService,
    ILogger<UserFunctions> logger)
{
    [Function("GetUser")]
    public async Task<HttpResponseData> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id:guid}")] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        // Implementation
    }
}

// Public static: Extension methods (must be public and static)
public static class HttpRequestExtensions
{
    public static async Task<HttpResponseData> CreateBadRequestResponseAsync(
        this HttpRequestData req,
        string code,
        string message)
    {
        // Implementation
    }
}
```

### Rationale

**`sealed` Benefits:**
- Compiler can perform devirtualization optimizations
- Makes code intentions explicit
- Prevents fragile base class problems
- Simplifies reasoning about code behavior
- Avoids Liskov Substitution Principle violations

**`internal` Benefits:**
- Follows principle of least privilege
- Forces use of abstractions (interfaces)
- Allows internal refactoring without versioning concerns
- Reduces public API surface
- Better encapsulation of implementation details

**Combined Benefits:**
- Clear separation between contract (public interface) and implementation (internal sealed class)
- Supports dependency injection patterns
- Enables easier unit testing (mock interfaces, not implementations)
- Improves maintainability and flexibility

### Service Implementation Pattern

**PREFERRED: Use Primary Constructors for Dependency Injection (C# 12+)**

```csharp
// ✅ Preferred: Primary constructor with internal sealed (C# 12+)
internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<UserResponse>> GetUserByIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user by ID: {UserId}", userId.Value);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId.Value);
            return Result.Failure<UserResponse>(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        var roles = await userManager.GetRolesAsync(user);
        var response = MapToUserResponse(user, roles);

        return Result.Success(response);
    }

    private static UserResponse MapToUserResponse(ApplicationUser user, IList<string> roles)
    {
        return new UserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        );
    }
}
```

**Alternative: Traditional Constructor (When Primary Constructor Not Available)**

```csharp
// ⚠️ Acceptable but not preferred: Traditional constructor
internal sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", userId.Value);
        // ... rest of implementation
    }
}
```

### Primary Constructor Benefits

1. **Less Boilerplate** - No need to declare private fields and assign them
2. **More Concise** - Constructor parameters are directly accessible
3. **Clearer Intent** - Dependencies are immediately visible at class declaration
4. **Modern C#** - Leverages C# 12 features

### Primary Constructor Rules

1. **Use primary constructors for all services with dependency injection**
2. **Access injected dependencies directly by parameter name (no `_` prefix)**
3. **Use camelCase for primary constructor parameters**
4. **If you need to store state beyond constructor parameters, use traditional fields**

### Azure Functions Route Constants

**CRITICAL RULE: All Azure Function Routes Must Be Defined as Public Constants**

**✅ ALWAYS DO THIS:**
```csharp
public sealed class UserFunctions(
    IUserService userService,
    ILogger<UserFunctions> logger)
{
    // Route constants - defined at the top of the class
    public const string Get = "users";
    public const string GetById = "users/{id:guid}";
    public const string Post = Get;  // Reuse base route
    public const string Put = GetById;  // Reuse route with ID
    public const string Delete = GetById;  // Reuse route with ID

    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Get)] HttpRequestData req,
        FunctionContext context)
    {
        // implementation
    }

    [Function("GetUser")]
    public async Task<HttpResponseData> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GetById)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        // implementation
    }

    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Post)] HttpRequestData req,
        FunctionContext context)
    {
        // implementation
    }

    [Function("UpdateUser")]
    public async Task<HttpResponseData> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Put)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        // implementation
    }

    [Function("DeleteUser")]
    public async Task<HttpResponseData> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Delete)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        // implementation
    }
}
```

**❌ NEVER DO THIS:**
```csharp
public sealed class UserFunctions(
    IUserService userService,
    ILogger<UserFunctions> logger)
{
    // Bad: Routes hardcoded in attributes
    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequestData req,
        FunctionContext context)
    {
        // implementation
    }

    [Function("GetUser")]
    public async Task<HttpResponseData> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id:guid}")] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        // implementation
    }
}
```

### Route Constants Rules

1. **Define all routes as `public const string` at the top of the Functions class**
   - Place immediately after the primary constructor or class fields
   - Group related routes together
   - Use clear, descriptive constant names

2. **Naming convention for route constants**
   - Use HTTP method as constant name: `Get`, `Post`, `Put`, `Delete`, `Patch`
   - For specific routes, append descriptor: `GetById`, `GetByName`, `GetActive`
   - Examples:
     - `Get` = base collection route
     - `GetById` = single resource route
     - `Post` = creation route (reuse `Get`)
     - `Put` = update route (reuse `GetById`)
     - `Delete` = deletion route (reuse `GetById`)

3. **Reuse constants where routes are identical**
   ```csharp
   public const string Get = "users";
   public const string Post = Get;        // Same route, different HTTP method
   public const string GetById = "users/{id:guid}";
   public const string Put = GetById;     // Same route, different HTTP method
   public const string Delete = GetById;  // Same route, different HTTP method
   ```

4. **Benefits of route constants**
   - **Single Source of Truth**: Route changes in one place
   - **Type Safety**: Compiler catches typos
   - **Discoverability**: Easy to see all endpoints in a class
   - **Testing**: Can reference constants in tests
   - **Documentation**: Routes are self-documenting
   - **Refactoring**: Easier to change route structure

### Complete Examples by Functions Class

**AuthFunctions:**
```csharp
public sealed class AuthFunctions(
    ILogger<AuthFunctions> logger,
    IAuthenticationService authService)
{
    // Route constants
    public const string Register = "auth/register";
    public const string Login = "auth/login";
    public const string Refresh = "auth/refresh";
    public const string Revoke = "auth/revoke";

    [Function("Register")]
    public async Task<HttpResponseData> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Register)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Processing registration request");
        // implementation
    }

    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Login)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Processing login request");
        // implementation
    }

    [Function("RefreshToken")]
    public async Task<HttpResponseData> RefreshToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Refresh)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Processing token refresh request");
        // implementation
    }
}
```

**RoleFunctions:**
```csharp
public sealed class RoleFunctions(
    IRoleService roleService,
    ILogger<RoleFunctions> logger)
{
    // Route constants
    public const string Get = "roles";
    public const string Post = Get;
    public const string GetById = "roles/{id:guid}";
    public const string Put = GetById;
    public const string Delete = GetById;
    public const string GetByName = "roles/name/{name}";
    public const string Assign = "roles/assign";
    public const string Remove = "roles/remove";
    public const string GetUsersInRole = "roles/{name}/users";

    [Function("GetAllRoles")]
    public async Task<HttpResponseData> GetAllRoles(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Get)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Getting all roles");
        // implementation
    }

    [Function("CreateRole")]
    public async Task<HttpResponseData> CreateRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Post)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Creating new role");
        // implementation
    }

    [Function("GetRoleById")]
    public async Task<HttpResponseData> GetRoleById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GetById)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        logger.LogInformation("Getting role {RoleId}", id);
        // implementation
    }

    [Function("UpdateRole")]
    public async Task<HttpResponseData> UpdateRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Put)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        logger.LogInformation("Updating role {RoleId}", id);
        // implementation
    }

    [Function("DeleteRole")]
    public async Task<HttpResponseData> DeleteRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Delete)] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        logger.LogInformation("Deleting role {RoleId}", id);
        // implementation
    }

    [Function("GetRoleByName")]
    public async Task<HttpResponseData> GetRoleByName(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GetByName)] HttpRequestData req,
        string name,
        FunctionContext context)
    {
        logger.LogInformation("Getting role by name: {RoleName}", name);
        // implementation
    }

    [Function("AssignRoleToUser")]
    public async Task<HttpResponseData> AssignRoleToUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Assign)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Assigning role to user");
        // implementation
    }

    [Function("RemoveRoleFromUser")]
    public async Task<HttpResponseData> RemoveRoleFromUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Remove)] HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Removing role from user");
        // implementation
    }

    [Function("GetUsersInRole")]
    public async Task<HttpResponseData> GetUsersInRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = GetUsersInRole)] HttpRequestData req,
        string name,
        FunctionContext context)
    {
        logger.LogInformation("Getting users in role: {RoleName}", name);
        // implementation
    }
}
```

### Route Constants in Tests

**Benefit**: Tests can reference the same constants to ensure consistency

```csharp
[TestMethod]
public void RouteConstants_AreDefinedCorrectly()
{
    // Arrange & Assert
    UserFunctions.Get.Should().Be("users");
    UserFunctions.GetById.Should().Be("users/{id:guid}");
    UserFunctions.Post.Should().Be(UserFunctions.Get);
    UserFunctions.Put.Should().Be(UserFunctions.GetById);
    UserFunctions.Delete.Should().Be(UserFunctions.GetById);
}

[TestMethod]
public void AuthFunctionRoutes_StartWithAuth()
{
    // Arrange & Assert
    AuthFunctions.Register.Should().StartWith("auth/");
    AuthFunctions.Login.Should().StartWith("auth/");
    AuthFunctions.Refresh.Should().StartWith("auth/");
}
```

### Route Constants Rationale

- **Maintainability**: Change route structure in one place
- **Consistency**: Ensures all endpoints using same route use same string
- **Discoverability**: Quick overview of all API endpoints in a class
- **Testability**: Can validate route structure in unit tests
- **Refactoring**: IDE can find all usages when renaming
- **Documentation**: Self-documenting API surface
- **Type Safety**: Compiler validates at build time instead of runtime

---

## Naming Conventions

### General Rules

1. **Use PascalCase for:**
   - Classes, interfaces, methods, properties, public fields
   - Example: `UserService`, `GetUserByIdAsync`, `FirstName`

2. **Use camelCase for:**
   - Private fields (with `_` prefix), local variables, parameters
   - Example: `_userManager`, `userId`, `request`

3. **Use meaningful names that describe intent**
   ```csharp
   // Good
   var user = await _userManager.FindByIdAsync(userId);

   // Bad
   var u = await _userManager.FindByIdAsync(id);
   var temp = await _userManager.FindByIdAsync(userId);
   ```

### Method Naming

1. **Async methods end with `Async`**
   ```csharp
   public async Task<Result<User>> GetUserAsync(UserId userId)
   ```

2. **Use verb phrases for methods**
   - `GetUserAsync`, `CreateRoleAsync`, `UpdateUserAsync`
   - `ValidateEmail`, `MapToUserResponse`

3. **Boolean methods start with `Is`, `Has`, `Can`**
   ```csharp
   public bool IsValidEmail(string email)
   public bool HasRole(string roleName)
   ```

### Interface Naming

```csharp
// Good: Start with 'I'
public interface IUserService
public interface IAuthenticationService

// Bad
public interface UserService // Missing 'I' prefix
```

---

## Code Formatting

### Method Declarations

**CRITICAL RULE: Methods and Their Arguments Must Be on the Same Line**

**✅ ALWAYS DO THIS:**
```csharp
// Good: Method name and all arguments on same line
public async Task<Result<UserResponse>> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default)
{
    // implementation
}

// Good: Short method with single parameter
public Result<Email> Create(string email)
{
    // implementation
}

// Good: Even with multiple parameters, keep on same line
public async Task<Result<UserResponse>> UpdateUserAsync(UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
    // implementation
}
```

**❌ NEVER DO THIS:**
```csharp
// Bad: Arguments split across multiple lines
public async Task<Result<UserResponse>> UpdateUserAsync(
    UserId userId,
    UpdateUserRequest request,
    CancellationToken cancellationToken = default)
{
    // implementation
}

// Bad: Opening parenthesis on same line, parameters below
public async Task<Result<UserResponse>> UpdateUserAsync(
    UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
    // implementation
}
```

### Formatting Rules

1. **Method signature must be on a single line**
   - Method name, parameters, and opening brace on consecutive lines
   - No line breaks between method name and parameters
   - Opening brace `{` on new line

2. **Exception: Only break if line exceeds readability**
   - If the line becomes unmanageably long (150+ characters), consider:
     - Refactoring to reduce parameter count
     - Using parameter objects/DTOs to group related parameters
   - Even then, prefer keeping signature intact

3. **Apply to all method types**
   - Public methods
   - Private methods
   - Static methods
   - Extension methods
   - Constructor signatures (including primary constructors)

### Examples

**Service Methods:**
```csharp
// ✅ Good
public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
{
    _logger.LogInformation("Processing login request for {Email}", request.Email);
    // implementation
}

// ❌ Bad
public async Task<Result<AuthenticationResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
{
    // implementation
}
```

**Azure Functions:**
```csharp
// ✅ Good
[Function("GetUser")]
public async Task<HttpResponseData> GetUser([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id:guid}")] HttpRequestData req, string id, FunctionContext context)
{
    // implementation
}

// ❌ Bad
[Function("GetUser")]
public async Task<HttpResponseData> GetUser(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id:guid}")] HttpRequestData req,
    string id,
    FunctionContext context)
{
    // implementation
}
```

**Primary Constructors:**
```csharp
// ✅ Good
public sealed class UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger) : IUserService
{
    // implementation
}

// ❌ Bad
public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserService> logger) : IUserService
{
    // implementation
}
```

### Rationale

- **Consistency** - Uniform style across the codebase makes code easier to scan
- **Compactness** - More code visible on screen without scrolling
- **Readability** - Method signatures are a single logical unit
- **Modern IDEs** - Editors handle long lines with horizontal scrolling and tooltips
- **Method Calls** - If method declarations fit on one line, method calls should too

### Method Chaining and Dot Operators

**CRITICAL RULE: Method Invocations with More Than 1 Dot Operator Must Be on Separate Lines**

**✅ ALWAYS DO THIS:**
```csharp
// Good: Single dot operator - stays on one line
var name = user.GetName();
var email = user.Email;
var result = service.ProcessAsync(request);

// Good: Multiple dot operators - each call on separate line
var result = user
    .GetSomething()
    .ToString();

var users = await _userManager.Users
    .Where(u => u.Email.Contains(searchTerm))
    .OrderBy(u => u.CreatedAt)
    .ToListAsync(cancellationToken);

var response = result
    .Value
    .MapToDto();

// Good: LINQ query with multiple operations
var activeUsers = users
    .Where(u => u.IsActive)
    .Select(u => u.Email)
    .ToList();

// Good: Builder pattern
var user = new UserBuilder()
    .WithEmail("test@example.com")
    .WithName("John", "Doe")
    .WithRole("Admin")
    .Build();
```

**❌ NEVER DO THIS:**
```csharp
// Bad: Multiple dot operators on same line
var result = user.GetSomething().ToString();

// Bad: Chained LINQ on one line
var users = await _userManager.Users.Where(u => u.Email.Contains(searchTerm)).OrderBy(u => u.CreatedAt).ToListAsync(cancellationToken);

// Bad: Property chain on one line
var value = result.Value.MapToDto();

// Bad: Builder pattern on one line
var user = new UserBuilder().WithEmail("test@example.com").WithName("John", "Doe").Build();
```

### Method Chaining Rules

1. **Count the dots in the expression**
   - 1 dot: Keep on same line
   - 2+ dots: Split across lines

2. **Indentation for chained calls**
   - First call at normal indentation
   - Subsequent calls indented one level
   - Each dot operator starts the line

3. **Apply to all chaining patterns**
   - LINQ queries (`.Where().Select().ToList()`)
   - Fluent APIs (`.WithX().WithY()`)
   - Property navigation (`.Result.Value.Property`)
   - Method chaining (`.Method1().Method2()`)

4. **Exception: Null-conditional operators**
   ```csharp
   // OK: Null-conditional with single method call
   var result = user?.GetName();

   // Split if multiple operations
   var result = user?.GetProfile()
       ?.Email;
   ```

### Chaining Examples

**Service Layer:**
```csharp
// ✅ Good: Single operation
var user = await _userManager.FindByIdAsync(userId.ToString());

// ✅ Good: Multiple operations split
var users = await _context.Users
    .Include(u => u.Roles)
    .Where(u => u.IsActive)
    .ToListAsync(cancellationToken);

// ❌ Bad: Multiple operations on one line
var users = await _context.Users.Include(u => u.Roles).Where(u => u.IsActive).ToListAsync(cancellationToken);
```

**Value Objects:**
```csharp
// ✅ Good: Single operation
var emailResult = Email.Create(request.Email);

// ✅ Good: Multiple operations split
var normalizedEmail = Email.Create(request.Email)
    .Value
    .ToString()
    .ToLowerInvariant();

// ❌ Bad: Chained on one line
var normalizedEmail = Email.Create(request.Email).Value.ToString().ToLowerInvariant();
```

**LINQ Queries:**
```csharp
// ✅ Good: Each operation on new line
var adminEmails = await _userManager.Users
    .Where(u => u.Roles.Contains("Admin"))
    .Select(u => u.Email)
    .OrderBy(e => e)
    .ToListAsync(cancellationToken);

// ❌ Bad: All on one line
var adminEmails = await _userManager.Users.Where(u => u.Roles.Contains("Admin")).Select(u => u.Email).OrderBy(e => e).ToListAsync(cancellationToken);
```

**Result Pattern:**
```csharp
// ✅ Good: Single check
if (result.IsFailure)
    return result.Error;

// ✅ Good: Multiple operations split
var errorCode = result
    .Error
    .Code
    .ToString();

// ❌ Bad: Chained property access
var errorCode = result.Error.Code.ToString();
```

### Chaining Rationale

- **Readability** - Each operation is clearly visible on its own line
- **Debugging** - Easier to set breakpoints on specific operations
- **Git Diffs** - Changes to specific operations show clearly in version control
- **Step-through** - Debugger can step through each operation individually
- **Understanding** - Complex chains are easier to comprehend when vertically aligned

### Vertical Spacing (Blank Lines)

**CRITICAL RULE: Use Blank Lines to Separate Logical Groupings of Code**

**✅ ALWAYS DO THIS:**
```csharp
// Good: Blank line between variable assignment and null check
var user = await userManager.FindByEmailAsync(request.Email);

if (user is null)
{
    return Result.Failure<AuthenticationResponse>(
        Error.NotFound("User.NotFound", "User not found"));
}

// Good: Blank lines separate logical sections
public async Task<Result<UserResponse>> UpdateUserAsync(UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
    // 1. Logging
    _logger.LogInformation("Updating user {UserId}", userId.Value);

    // 2. Fetch entity
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if (user is null)
    {
        return Result.Failure<UserResponse>(
            Error.NotFound("User.NotFound", $"User with ID {userId} not found"));
    }

    // 3. Update properties
    if (!string.IsNullOrWhiteSpace(request.FirstName))
    {
        user.FirstName = request.FirstName;
    }

    if (!string.IsNullOrWhiteSpace(request.LastName))
    {
        user.LastName = request.LastName;
    }

    user.LastModifiedAt = DateTime.UtcNow;

    // 4. Persist changes
    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        return Result.Failure<UserResponse>(
            Error.Internal("User.UpdateFailed", "Failed to update user"));
    }

    // 5. Return response
    var roles = await _userManager.GetRolesAsync(user);
    var response = MapToUserResponse(user, roles);

    return Result.Success(response);
}
```

**❌ NEVER DO THIS:**
```csharp
// Bad: No spacing between logical operations
var user = await userManager.FindByEmailAsync(request.Email);
if (user is null)
{
    return Result.Failure<AuthenticationResponse>(
        Error.NotFound("User.NotFound", "User not found"));
}
var roles = await userManager.GetRolesAsync(user);
var response = MapToResponse(user, roles);
return Result.Success(response);

// Bad: Cramped code with no visual separation
public async Task<Result<UserResponse>> UpdateUserAsync(UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
    _logger.LogInformation("Updating user {UserId}", userId.Value);
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user is null)
    {
        return Result.Failure<UserResponse>(
            Error.NotFound("User.NotFound", $"User with ID {userId} not found"));
    }
    if (!string.IsNullOrWhiteSpace(request.FirstName))
    {
        user.FirstName = request.FirstName;
    }
    user.LastModifiedAt = DateTime.UtcNow;
    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
        return Result.Failure<UserResponse>(
            Error.Internal("User.UpdateFailed", "Failed to update user"));
    }
    var roles = await _userManager.GetRolesAsync(user);
    return Result.Success(MapToUserResponse(user, roles));
}
```

### Vertical Spacing Rules

1. **Always add blank line before control flow statements**
   ```csharp
   // Good
   var user = await _userManager.FindByIdAsync(userId.ToString());

   if (user is null)
       return Result.Failure(...);

   // Bad
   var user = await _userManager.FindByIdAsync(userId.ToString());
   if (user is null)
       return Result.Failure(...);
   ```

2. **Separate logical sections with blank lines**
   - Variable declarations/initialization
   - Validation checks
   - Business logic operations
   - Data persistence
   - Result construction

3. **Blank line after opening brace when method starts with comment**
   ```csharp
   public async Task<Result<User>> GetUserAsync(UserId userId)
   {
       // Fetch user from database

       var user = await _userManager.FindByIdAsync(userId.ToString());
       // ...
   }
   ```

4. **No blank line needed for very short methods**
   ```csharp
   // OK: Simple one-liner or two-liner
   public bool IsEmpty(string value)
   {
       return string.IsNullOrWhiteSpace(value);
   }
   ```

5. **Group related if statements without blank lines**
   ```csharp
   // Good: Related null checks together
   if (request is null)
       return Result.Failure(...);
   if (string.IsNullOrEmpty(request.Email))
       return Result.Failure(...);

   // Then blank line before next logical section
   var user = await _userManager.FindByEmailAsync(request.Email);
   ```

6. **Blank line before return statement (in most cases)**
   ```csharp
   // Good
   var response = MapToUserResponse(user, roles);

   return Result.Success(response);

   // Exception: Early returns for validation can be compact
   if (user is null)
       return Result.Failure(...);
   ```

### Spacing Examples by Pattern

**Validation Pattern:**
```csharp
// ✅ Good
public async Task<Result<UserResponse>> GetUserAsync(UserId userId)
{
    _logger.LogInformation("Getting user {UserId}", userId.Value);

    var user = await _userManager.FindByIdAsync(userId.ToString());

    if (user is null)
    {
        _logger.LogWarning("User not found: {UserId}", userId.Value);
        return Result.Failure<UserResponse>(
            Error.NotFound("User.NotFound", $"User not found"));
    }

    var roles = await _userManager.GetRolesAsync(user);
    var response = MapToUserResponse(user, roles);

    return Result.Success(response);
}
```

**Error Handling Pattern:**
```csharp
// ✅ Good
var result = await _userManager.CreateAsync(user, request.Password);

if (!result.Succeeded)
{
    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
    _logger.LogWarning("User creation failed: {Errors}", errors);

    return Result.Failure<AuthenticationResponse>(
        Error.Internal("User.CreationFailed", errors));
}

_logger.LogInformation("User created successfully: {UserId}", user.Id);
```

**Multiple Operations:**
```csharp
// ✅ Good
var user = await _userManager.FindByIdAsync(userId.ToString());

if (user is null)
    return Result.Failure<Unit>(Error.NotFound("User.NotFound", "User not found"));

var roles = await _userManager.GetRolesAsync(user);

foreach (var role in roles)
{
    await _userManager.RemoveFromRoleAsync(user, role);
}

var deleteResult = await _userManager.DeleteAsync(user);

if (!deleteResult.Succeeded)
    return Result.Failure<Unit>(Error.Internal("User.DeleteFailed", "Failed to delete user"));

return Result.Success(Unit.Value);
```

### Spacing Rationale

- **Readability** - Code "breathes" and is easier to scan visually
- **Logical Grouping** - Related statements are grouped together
- **Reduced Cognitive Load** - Blank lines act as visual separators between thoughts
- **Bug Prevention** - Easier to spot missing null checks or validation
- **Code Review** - Reviewers can quickly understand code flow

### Control Flow Braces

**CRITICAL RULE: Always Use Braces for Control Flow Statements**

**✅ ALWAYS DO THIS:**
```csharp
// Good: Braces for if statement even with single statement
if (user is null)
{
    return Result.Failure<UserResponse>(
        Error.NotFound("User.NotFound", "User not found"));
}

// Good: Braces for for loop
for (int i = 0; i < count; i++)
{
    ProcessItem(i);
}

// Good: Braces for foreach loop
foreach (var role in roles)
{
    await AssignRoleAsync(user, role);
}

// Good: Braces for while loop
while (hasMoreData)
{
    data = await FetchNextBatchAsync();
}

// Good: Braces for else clause
if (result.IsSuccess)
{
    return result.Value;
}
else
{
    return HandleError(result.Error);
}

// Good: Braces for nested statements
if (user is not null)
{
    if (user.IsActive)
    {
        await ProcessActiveUserAsync(user);
    }
}
```

**❌ NEVER DO THIS:**
```csharp
// Bad: No braces for single statement if
if (user is null)
    return Result.Failure<UserResponse>(Error.NotFound("User.NotFound", "User not found"));

// Bad: No braces for for loop
for (int i = 0; i < count; i++)
    ProcessItem(i);

// Bad: No braces for foreach
foreach (var role in roles)
    await AssignRoleAsync(user, role);

// Bad: No braces for while
while (hasMoreData)
    data = await FetchNextBatchAsync();

// Bad: Mixed braces (if has braces but else doesn't)
if (result.IsSuccess)
{
    return result.Value;
}
else
    return HandleError(result.Error);

// Bad: Inline if statement
if (user is null) return Result.Failure(...);
```

### Braces Rules

1. **Always use braces for all control flow statements**
   - `if`, `else`, `else if`
   - `for`, `foreach`, `while`, `do-while`
   - Even when the body contains only a single statement
   - Even for single-line statements

2. **Opening brace placement**
   ```csharp
   // Good: Opening brace on new line
   if (condition)
   {
       // statement
   }

   // Also acceptable: Same line (if project prefers K&R style)
   if (condition) {
       // statement
   }

   // For consistency with the rest of the codebase, prefer new line
   ```

3. **Closing brace placement**
   - Always on its own line at the same indentation as the control statement
   - Exception: `else`, `else if`, `catch`, `finally` can follow closing brace

4. **Apply to all control structures**
   - `if`/`else if`/`else`
   - `for`/`foreach`/`while`/`do-while`
   - `switch` cases (already require braces)
   - `try`/`catch`/`finally` (already require braces)

### Examples by Control Structure

**If Statements:**
```csharp
// ✅ Good
if (user is null)
{
    logger.LogWarning("User not found");
    return Result.Failure<UserResponse>(Error.NotFound("User.NotFound", "User not found"));
}

// ✅ Good: Single statement still has braces
if (request is null)
{
    return BadRequest();
}

// ❌ Bad: No braces
if (user is null)
    return Result.Failure(...);
```

**Loops:**
```csharp
// ✅ Good: for loop
for (int i = 0; i < users.Count; i++)
{
    await ProcessUserAsync(users[i]);
}

// ✅ Good: foreach loop
foreach (var role in rolesToAssign)
{
    await userManager.AddToRoleAsync(user, role);
}

// ✅ Good: while loop
while (reader.Read())
{
    results.Add(reader.GetString(0));
}

// ❌ Bad: No braces
for (int i = 0; i < count; i++)
    ProcessItem(i);
```

**If-Else Chains:**
```csharp
// ✅ Good: All branches have braces
if (result.IsSuccess)
{
    return result.Value;
}
else if (result.Error.Type == ErrorType.NotFound)
{
    return NotFound();
}
else
{
    return InternalServerError();
}

// ❌ Bad: Missing braces on else
if (result.IsSuccess)
{
    return result.Value;
}
else
    return InternalServerError();
```

**Nested Statements:**
```csharp
// ✅ Good: All levels have braces
if (user is not null)
{
    if (user.IsActive)
    {
        await ProcessUserAsync(user);
    }
    else
    {
        logger.LogWarning("Inactive user: {UserId}", user.Id);
    }
}

// ❌ Bad: Inner statement without braces
if (user is not null)
{
    if (user.IsActive)
        await ProcessUserAsync(user);
}
```

**Early Returns:**
```csharp
// ✅ Good: Even early returns have braces
if (request is null)
{
    return BadRequest();
}

if (user is null)
{
    return NotFound();
}

// ❌ Bad: No braces for early return
if (request is null)
    return BadRequest();
```

### Braces Rationale

1. **Bug Prevention** - The most important reason
   ```csharp
   // Dangerous without braces - easy to add a second line and forget braces
   if (user is null)
       logger.LogWarning("User not found");
       return NotFound(); // BUG: This always executes!

   // Safe with braces - compiler enforces scope
   if (user is null)
   {
       logger.LogWarning("User not found");
       return NotFound(); // Correctly inside the if block
   }
   ```

2. **Consistency** - All control structures look the same, reducing cognitive load

3. **Maintainability** - Adding/removing statements doesn't require adding braces

4. **Code Reviews** - Easier to spot logical errors and scope issues

5. **Refactoring Safety** - Less likely to introduce bugs when modifying code

6. **Debugging** - Clearer scope boundaries for breakpoints

7. **Industry Standard** - Recommended by Microsoft C# coding conventions

### When Braces Are Required (Compile Error Without Them)

These already require braces by C# syntax:
- Method bodies
- Class/interface/struct definitions
- `switch` statement bodies (not individual cases)
- `try`/`catch`/`finally` blocks
- Lambda expressions with multiple statements
- Anonymous methods

### When This Rule Applies (Braces Optional But Required By This Standard)

This rule mandates braces for:
- `if`, `else`, `else if` statements
- `for` loops
- `foreach` loops
- `while` loops
- `do-while` loops
- Single statement cases where braces are syntactically optional

---

## Async/Await Best Practices

### Rules

1. **Always use `async`/`await` for async operations**
   ```csharp
   // Good
   public async Task<Result<User>> GetUserAsync(UserId userId)
   {
       var user = await _userManager.FindByIdAsync(userId.ToString());
       return Result.Success(user);
   }

   // Bad: Don't return Task directly unless you have a good reason
   public Task<Result<User>> GetUserAsync(UserId userId)
   {
       return _userManager.FindByIdAsync(userId.ToString());
   }
   ```

2. **Always accept CancellationToken as the last parameter**
   ```csharp
   public async Task<Result<User>> GetUserAsync(
       UserId userId,
       CancellationToken cancellationToken = default)
   {
       var user = await _userManager.FindByIdAsync(userId.ToString());
       return Result.Success(user);
   }
   ```

3. **Pass CancellationToken to underlying async methods**
   ```csharp
   var users = await _userManager.Users.ToListAsync(cancellationToken);
   ```

4. **Never use `.Result` or `.Wait()` on async methods**
   ```csharp
   // Bad
   var user = GetUserAsync(userId).Result; // NEVER do this - causes deadlocks

   // Good
   var user = await GetUserAsync(userId);
   ```

5. **Use `ConfigureAwait(false)` in library code (not required for Azure Functions)**
   - Azure Functions isolated worker model doesn't require `ConfigureAwait(false)`
   - Use if building a reusable library

---

## Logging Standards

### Logging Levels

1. **LogInformation** - Normal flow events
   ```csharp
   _logger.LogInformation("Getting user by ID: {UserId}", userId.Value);
   _logger.LogInformation("User created successfully: {UserId}", user.Id);
   ```

2. **LogWarning** - Recoverable errors, business logic failures
   ```csharp
   _logger.LogWarning("User not found: {UserId}", userId.Value);
   _logger.LogWarning("Login failed: Invalid credentials for user {UserId}", user.Id);
   ```

3. **LogError** - Unexpected errors, exceptions
   ```csharp
   _logger.LogError(ex, "Failed to update user {UserId}", userId.Value);
   ```

4. **LogDebug** - Detailed diagnostic information (development only)
   ```csharp
   _logger.LogDebug("Validating email: {Email}", email);
   ```

### Logging Rules

1. **Always use structured logging with parameters**
   ```csharp
   // Good
   _logger.LogInformation("User {UserId} updated by {ActorId}", userId, actorId);

   // Bad
   _logger.LogInformation($"User {userId} updated by {actorId}"); // String interpolation loses structure
   ```

2. **Never log sensitive information**
   - Don't log: passwords, tokens, credit cards, SSN
   - OK to log: user IDs, email addresses (in development), operation names

3. **Log at the service layer, not in domain entities**

---

## Validation Rules

### Input Validation Pattern

```csharp
public async Task<Result<UserResponse>> UpdateUserAsync(
    UserId userId,
    UpdateUserRequest request,
    CancellationToken cancellationToken = default)
{
    // 1. Validate input parameters
    if (request is null)
    {
        return Result.Failure<UserResponse>(
            Error.Validation("Request.Null", "Request cannot be null"));
    }

    // 2. Validate business rules
    if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Length > 100)
    {
        return Result.Failure<UserResponse>(
            Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters"));
    }

    // 3. Check entity exists
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user is null)
    {
        return Result.Failure<UserResponse>(
            Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
    }

    // 4. Perform operation
    // ... update logic
}
```

### Validation Rules

1. **Validate early and return early**
   - Check null first
   - Validate input format
   - Check business rules
   - Verify entity exists

2. **Use value objects for domain validation**
   ```csharp
   var emailResult = Email.Create(request.Email);
   if (emailResult.IsFailure)
       return Result.Failure<UserResponse>(emailResult.Error!);
   ```

3. **Provide specific error messages**
   ```csharp
   // Good
   Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters")

   // Bad
   Error.Validation("ValidationError", "Invalid input") // Too generic
   ```

---

## Testing Requirements

### Unit Test Standards

1. **Use AAA pattern (Arrange, Act, Assert)**
   ```csharp
   [TestMethod]
   public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
   {
       // Arrange
       var userId = UserId.New();
       var expectedUser = new ApplicationUser { Id = userId.Value };
       _userManagerMock.Setup(x => x.FindByIdAsync(userId.Value.ToString()))
           .ReturnsAsync(expectedUser);

       // Act
       var result = await _sut.GetUserByIdAsync(userId);

       // Assert
       result.IsSuccess.Should().BeTrue();
       result.Value.Should().NotBeNull();
   }
   ```

2. **Test method naming: `MethodName_Scenario_ExpectedResult`**
   ```csharp
   GetUserByIdAsync_WithValidId_ReturnsSuccess()
   GetUserByIdAsync_WithInvalidId_ReturnsNotFoundError()
   CreateRole_WithDuplicateName_ReturnsConflictError()
   ```

3. **One assertion per test (or related assertions)**

4. **Use FluentAssertions for readable assertions**
   ```csharp
   result.IsSuccess.Should().BeTrue();
   result.Value.Email.Should().Be("test@example.com");
   users.Should().HaveCount(3);
   ```

---

## Code Review Checklist

Before submitting code, verify:

- [ ] No nullable warnings suppressed with `!` operator
- [ ] All null checks performed explicitly
- [ ] Result pattern used for all service methods
- [ ] Proper error types used (NotFound, Validation, Conflict, etc.)
- [ ] Implementation classes marked `internal sealed`
- [ ] Azure Functions have route constants defined
- [ ] Route constants reused where appropriate (Post = Get, Put = GetById, etc.)
- [ ] Route constants use ALL_CAPS naming convention
- [ ] Method signatures on single line (no multi-line parameter lists)
- [ ] Method chains with 2+ dots split across lines (one operation per line)
- [ ] Blank lines separate logical code sections
- [ ] Blank line before control flow statements (if, for, while, etc.)
- [ ] All control flow statements use braces (if, for, foreach, while, etc.)
- [ ] Async methods end with `Async`
- [ ] CancellationToken accepted and passed through
- [ ] Structured logging used with parameters
- [ ] No sensitive data logged
- [ ] Input validation performed
- [ ] Early returns for error cases
- [ ] Clean architecture layers respected
- [ ] Meaningful variable and method names
- [ ] XML documentation comments on public members
- [ ] Tests follow AAA pattern

---

## Examples

### Complete Service Method Example

```csharp
internal sealed class RoleService(
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<RoleService> logger) : IRoleService
{
    /// <summary>
    /// Creates a new role
    /// </summary>
    public async Task<Result<RoleResponse>> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
    _logger.LogInformation("Creating role: {RoleName}", request.Name);

    // Validate input
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Result.Failure<RoleResponse>(
            Error.Validation("Role.NameRequired", "Role name is required"));
    }

    // Check for conflicts
    var existingRole = await _roleManager.FindByNameAsync(request.Name);
    if (existingRole is not null)
    {
        _logger.LogWarning("Role creation failed: Role {RoleName} already exists", request.Name);
        return Result.Failure<RoleResponse>(
            Error.Conflict("Role.AlreadyExists", $"Role '{request.Name}' already exists"));
    }

    // Create entity
    var role = new IdentityRole<Guid>
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        NormalizedName = request.Name.ToUpperInvariant()
    };

    // Persist
    var result = await _roleManager.CreateAsync(role);
    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("Role creation failed: {Errors}", errors);
        return Result.Failure<RoleResponse>(
            Error.Internal("Role.CreationFailed", errors));
    }

    logger.LogInformation("Role created successfully: {RoleId}", role.Id);

    var response = new RoleResponse(role.Id, role.Name!, 0);
    return Result.Success(response);
    }
}
```

### Complete Azure Function Example

```csharp
public sealed class RoleFunctions(
    IRoleService roleService,
    ILogger<RoleFunctions> logger)
{
    [Function("CreateRole")]
    public async Task<HttpResponseData> CreateRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "roles")]
        HttpRequestData req,
        FunctionContext context)
    {
        logger.LogInformation("Creating new role");

    // Validate request body
    var request = await req.ReadFromJsonAsync<CreateRoleRequest>();
    if (request is null)
    {
        return await req.CreateBadRequestResponseAsync("Request.Invalid", "Invalid request body");
    }

    // Delegate to service
    var result = await roleService.CreateRoleAsync(request, context.CancellationToken);

    // Map result to HTTP response
    if (result.IsFailure)
    {
        return result.Error!.Type switch
        {
            ErrorType.Validation => await req.CreateBadRequestResponseAsync(
                result.Error.Code, result.Error.Message),
            ErrorType.Conflict => await req.CreateConflictResponseAsync(
                result.Error.Code, result.Error.Message),
            _ => await req.CreateInternalServerErrorResponseAsync(
                result.Error.Code, "Failed to create role")
        };
    }

    var response = req.CreateResponse(HttpStatusCode.Created);
    await response.WriteAsJsonAsync(result.Value);
    return response;
    }
}
```

---

## Summary

**The Golden Rules:**

1. ⛔ **Never suppress nullable warnings** - Always check for null explicitly
2. ✅ **Always use the Result pattern** - No exceptions for business logic
3. 🔒 **Seal implementations** - All implementation classes must be `internal sealed`
4. 🛣️ **Route constants required** - Azure Functions must define route constants (ALL_CAPS)
5. 📐 **Methods on single line** - Keep method signatures compact and readable
6. ⛓️ **Split method chains** - Multiple dot operators require separate lines
7. 📏 **Use vertical spacing** - Blank lines separate logical code sections
8. 🔐 **Always use braces** - All control flow statements must have braces
9. 📝 **Log everything with context** - Structured logging with parameters
10. 🔍 **Validate early, return early** - Check inputs before processing
11. 🏗️ **Respect clean architecture** - Dependencies flow inward only
12. ⚡ **Async all the way** - Use `async`/`await` consistently
13. 🧪 **Test everything** - Unit tests for all business logic

Following these standards ensures consistent, maintainable, and safe code across the entire UserManagement application.
