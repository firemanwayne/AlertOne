using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
internal sealed class AuthenticationService(
    ITokenService tokenService,
    IDeviceService deviceService,
    IUserSettingsService userSettingsService,
    ILogger<AuthenticationService> logger,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext dbContext) : IAuthenticationService
{
    /// <inheritdoc />
    public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing registration for email: {Email}", request.Email);

        // Validate email
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(emailResult.Error!);
        }

        // Check if user exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
            return Result.Failure<AuthenticationResponse>(
                Error.Conflict("User.AlreadyExists", "User with this email already exists"));
        }

        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            return Result.Failure<AuthenticationResponse>(
                Error.Validation("Password.Mismatch", "Passwords do not match"));
        }

        // Validate first and last name
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure<AuthenticationResponse>(
                Error.Validation("User.NameRequired", "First name and last name are required"));
        }

        // Create user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            logger.LogWarning("User creation failed: {Errors}", errors);
            return Result.Failure<AuthenticationResponse>(
                Error.Validation("User.CreationFailed", errors));
        }

        // Assign default role
        await userManager.AddToRoleAsync(user, "User");

        // Create default user settings
        var userId = UserId.From(user.Id);
        await userSettingsService.CreateDefaultAsync(userId, cancellationToken);

        logger.LogInformation("User registered successfully: {UserId}", user.Id);

        // Verify email is set (should never be null after successful creation)
        if (string.IsNullOrEmpty(user.Email))
        {
            logger.LogError("User created but email is null: {UserId}", user.Id);
            return Result.Failure<AuthenticationResponse>(
                Error.Internal("User.InvalidState", "User was created but email is missing"));
        }

        // Register device and generate refresh token
        var deviceResult = await deviceService.RegisterOrUpdateDeviceAsync(userId, request.Device, null, cancellationToken);

        if (deviceResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(deviceResult.Error!);
        }

        var refreshToken = deviceResult.Value.RefreshToken;
        var deviceSession = deviceResult.Value.Device;

        // Generate access token
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, roles);

        var userResponse = new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        );

        var claims = BuildClaims(user.Id, user.Email, user.FirstName, user.LastName, roles);

        var response = new AuthenticationResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            claims,
            deviceSession
        );

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing login for email: {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
            return Result.Failure<AuthenticationResponse>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password"));
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                logger.LogWarning("Login failed: Account locked for user {UserId}", user.Id);
                return Result.Failure<AuthenticationResponse>(
                    Error.Forbidden("Auth.LockedOut", "Account is locked due to multiple failed login attempts"));
            }

            logger.LogWarning("Login failed: Invalid credentials for user {UserId}", user.Id);

            return Result.Failure<AuthenticationResponse>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password"));
        }

        logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        // Verify email is set (defensive check)
        if (string.IsNullOrEmpty(user.Email))
        {
            logger.LogError("User email is null: {UserId}", user.Id);
            return Result.Failure<AuthenticationResponse>(
                Error.Internal("User.InvalidState", "User email is missing"));
        }

        // Register device and generate refresh token
        var userId = UserId.From(user.Id);
        var deviceResult = await deviceService.RegisterOrUpdateDeviceAsync(userId, request.Device, null, cancellationToken);

        if (deviceResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(deviceResult.Error!);
        }

        var refreshToken = deviceResult.Value.RefreshToken;
        var deviceSession = deviceResult.Value.Device;

        // Generate access token
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, roles);

        var userResponse = new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        );

        var claims = BuildClaims(user.Id, user.Email, user.FirstName, user.LastName, roles);

        var response = new AuthenticationResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            claims,
            deviceSession
        );

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing token refresh");

        // Validate refresh token and get device
        var deviceValidationResult = await deviceService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (deviceValidationResult.IsFailure)
        {
            logger.LogWarning("Token refresh failed: Invalid refresh token");

            return Result.Failure<AuthenticationResponse>(deviceValidationResult.Error!);
        }

        var userId = deviceValidationResult.Value.UserId;
        var deviceId = deviceValidationResult.Value.DeviceId;

        // Get user
        var user = await userManager.FindByIdAsync(userId.Value.ToString());

        if (user is null)
        {
            logger.LogWarning("Token refresh failed: User not found {UserId}", userId.Value);

            return Result.Failure<AuthenticationResponse>(
                Error.Unauthorized("Auth.UserNotFound", "User not found"));
        }

        // Verify email is set (defensive check)
        if (string.IsNullOrEmpty(user.Email))
        {
            logger.LogError("User email is null: {UserId}", user.Id);

            return Result.Failure<AuthenticationResponse>(
                Error.Internal("User.InvalidState", "User email is missing"));
        }

        logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

        // Update device last activity
        await deviceService.UpdateLastActivityAsync(deviceId, cancellationToken);

        // Generate new tokens
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, roles);

        // Get device info
        var deviceResult = await deviceService.GetDeviceByIdAsync(userId, deviceId, cancellationToken);

        if (deviceResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(deviceResult.Error!);
        }

        var userResponse = new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        );

        var claims = BuildClaims(user.Id, user.Email, user.FirstName, user.LastName, roles);

        var response = new AuthenticationResponse(
            accessToken,
            request.RefreshToken,
            DateTime.UtcNow.AddMinutes(60),
            userResponse,
            claims,
            deviceResult.Value
        );

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result> RevokeTokenAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Revoking all devices for user: {UserId}", userId.Value);

        var user = await userManager.FindByIdAsync(userId.Value.ToString());

        if (user is null)
        {
            logger.LogWarning("Token revocation failed: User not found {UserId}", userId.Value);

            return Result.Failure(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        // Revoke all devices for the user
        var revokeResult = await deviceService.RevokeAllDevicesAsync(userId, cancellationToken);

        if (revokeResult.IsFailure)
        {
            return revokeResult;
        }

        logger.LogInformation("All devices revoked successfully for user: {UserId}", userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<bool>> HasUsersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if any users exist");

        try
        {
            // Simple query - for a new system with no/few users this is efficient
            var users = await dbContext.Users
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            var hasUsers = users.Count > 0;

            return Result.Success(hasUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if users exist");

            // If query fails, assume no users (allows registration)
            return Result.Success(false);
        }
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IEnumerable<UserClaim> BuildClaims(Guid userId, string email, string firstName, string lastName, IList<string> roles)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        var claims = new List<UserClaim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("firstName", firstName),
            new("lastName", lastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new UserClaim("role", role));
        }

        return claims;
    }
}
