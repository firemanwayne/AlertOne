using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Services;

/// <summary>
/// Service for user management operations
/// </summary>
internal sealed class UserService(ILogger<UserService> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IUserSettingsService userSettingsService) : IUserService
{
    public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating new user with email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<UserResponse>(emailResult.Error!);
        }

        var existingUser = await userManager.FindByEmailAsync(emailResult.Value.Value);

        if (existingUser is not null)
        {
            logger.LogWarning("User creation failed: Email {Email} already exists", request.Email);

            return Result.Failure<UserResponse>(
                Error.Conflict("User.AlreadyExists", $"User with email {request.Email} already exists"));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = emailResult.Value.Value,
            Email = emailResult.Value.Value,
            NormalizedUserName = emailResult.Value.Value.ToUpperInvariant(),
            NormalizedEmail = emailResult.Value.Value.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            logger.LogWarning("User creation failed: {Errors}", errors);

            return Result.Failure<UserResponse>(
                Error.Internal("User.CreationFailed", errors));
        }

        var rolesToAssign = request.Roles ?? new[] { "User" };

        foreach (var roleName in rolesToAssign)
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Errors}", roleName, user.Id, errors);
            }
        }

        logger.LogInformation("User created successfully: {UserId}", user.Id);

        // Create default user settings
        var userId = UserId.From(user.Id);

        await userSettingsService.CreateDefaultAsync(userId, cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var response = MapToUserResponse(user, roles);

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user by ID: {UserId}", userId.Value);

        var user = await userManager.FindByIdAsync(userId.Value.ToString());

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

    public async Task<Result<UserResponse>> GetUserByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user by email: {Email}", email.Value);

        var user = await userManager.FindByEmailAsync(email.Value);
        if (user is null)
        {
            logger.LogWarning("User not found with email: {Email}", email.Value);
            return Result.Failure<UserResponse>(
                Error.NotFound("User.NotFound", $"User with email {email.Value} not found"));
        }

        var roles = await userManager.GetRolesAsync(user);
        var response = MapToUserResponse(user, roles);

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> UpdateUserAsync(UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating user: {UserId}", userId.Value);

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId.Value);
            return Result.Failure<UserResponse>(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        // Update first name if provided
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            if (request.FirstName.Length > 100)
            {
                return Result.Failure<UserResponse>(
                    Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters"));
            }

            user.FirstName = request.FirstName.Trim();
        }

        // Update last name if provided
        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            if (request.LastName.Length > 100)
            {
                return Result.Failure<UserResponse>(
                    Error.Validation("User.LastNameTooLong", "Last name must not exceed 100 characters"));
            }
            user.LastName = request.LastName.Trim();
        }

        user.LastModifiedAt = DateTime.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));

            logger.LogWarning("User update failed: {Errors}", errors);

            return Result.Failure<UserResponse>(
                Error.Internal("User.UpdateFailed", errors));
        }

        logger.LogInformation("User updated successfully: {UserId}", userId.Value);

        var roles = await userManager.GetRolesAsync(user);

        var response = MapToUserResponse(user, roles);

        return Result.Success(response);
    }

    public async Task<Result> DeleteUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting user: {UserId}", userId.Value);

        var user = await userManager.FindByIdAsync(userId.Value.ToString());

        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId.Value);

            return Result.Failure(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        var deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));

            logger.LogWarning("User deletion failed: {Errors}", errors);

            return Result.Failure(
                Error.Internal("User.DeleteFailed", errors));
        }

        logger.LogInformation("User deleted successfully: {UserId}", userId.Value);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all users");

        // Note: userManager.Users requires IQueryableUserStore which we don't implement for Cosmos DB
        // Query the DbContext directly instead
        var users = await dbContext.Users.ToListAsync(cancellationToken);

        var userResponses = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            userResponses.Add(MapToUserResponse(user, roles));
        }

        return Result.Success<IEnumerable<UserResponse>>(userResponses);
    }

    private static UserResponse MapToUserResponse(ApplicationUser user, IList<string> roles)
    {
        // Email should always be set for Identity users, but provide defensive fallback
        var email = user.Email ?? string.Empty;

        return new UserResponse(
            user.Id,
            email,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        );
    }
}
