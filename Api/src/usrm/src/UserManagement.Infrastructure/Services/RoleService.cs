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
/// Service for role management operations
/// </summary>
internal sealed class RoleService(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ILogger<RoleService> logger) : IRoleService
{
    /// <inheritdoc />
    public async Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating role: {RoleName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<RoleResponse>(
                Error.Validation("Role.NameRequired", "Role name is required"));
        }

        // Check if role already exists
        var existingRole = await roleManager.FindByNameAsync(request.Name);
        if (existingRole is not null)
        {
            logger.LogWarning("Role creation failed: Role {RoleName} already exists", request.Name);
            return Result.Failure<RoleResponse>(
                Error.Conflict("Role.AlreadyExists", $"Role '{request.Name}' already exists"));
        }

        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Role creation failed: {Errors}", errors);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.CreationFailed", errors));
        }

        logger.LogInformation("Role created successfully: {RoleId}", role.Id);

        // Role.Name should always be set after creation, but provide defensive check
        if (string.IsNullOrEmpty(role.Name))
        {
            logger.LogError("Role created but Name is null: {RoleId}", role.Id);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.InvalidState", "Role was created but name is missing"));
        }

        var response = new RoleResponse(role.Id, role.Name, 0);
        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting role by ID: {RoleId}", roleId);

        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleId}", roleId);
            return Result.Failure<RoleResponse>(
                Error.NotFound("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Defensive check for role name
        if (string.IsNullOrEmpty(role.Name))
        {
            logger.LogError("Role has null Name: {RoleId}", roleId);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.InvalidState", "Role name is missing"));
        }

        var userCount = await GetUserCountInRoleAsync(role.Name);
        var response = new RoleResponse(role.Id, role.Name, userCount);

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<RoleResponse>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting role by name: {RoleName}", roleName);

        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleName}", roleName);
            return Result.Failure<RoleResponse>(
                Error.NotFound("Role.NotFound", $"Role '{roleName}' not found"));
        }

        // Defensive check for role name
        if (string.IsNullOrEmpty(role.Name))
        {
            logger.LogError("Role has null Name: {RoleId}", role.Id);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.InvalidState", "Role name is missing"));
        }

        var userCount = await GetUserCountInRoleAsync(role.Name);
        var response = new RoleResponse(role.Id, role.Name, userCount);

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all roles");

        // Note: roleManager.Roles requires IQueryableRoleStore which we don't implement for Cosmos DB
        // Query the DbContext directly instead
        var roles = await dbContext.Roles.ToListAsync(cancellationToken);

        var responses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            // Skip roles with null names (defensive - should never happen)
            if (string.IsNullOrEmpty(role.Name))
            {
                logger.LogWarning("Skipping role with null Name: {RoleId}", role.Id);
                continue;
            }

            var userCount = await GetUserCountInRoleAsync(role.Name);
            responses.Add(new RoleResponse(role.Id, role.Name, userCount));
        }

        return Result.Success<IEnumerable<RoleResponse>>(responses);
    }

    /// <inheritdoc />
    public async Task<Result<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating role: {RoleId}", roleId);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<RoleResponse>(
                Error.Validation("Role.NameRequired", "Role name is required"));
        }

        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleId}", roleId);
            return Result.Failure<RoleResponse>(
                Error.NotFound("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Check if new name already exists (excluding current role)
        var existingRole = await roleManager.FindByNameAsync(request.Name);
        if (existingRole is not null && existingRole.Id != roleId)
        {
            logger.LogWarning("Role update failed: Role name {RoleName} already exists", request.Name);
            return Result.Failure<RoleResponse>(
                Error.Conflict("Role.NameTaken", $"Role name '{request.Name}' is already taken"));
        }

        role.Name = request.Name;
        role.NormalizedName = request.Name.ToUpperInvariant();

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Role update failed: {Errors}", errors);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.UpdateFailed", errors));
        }

        logger.LogInformation("Role updated successfully: {RoleId}", roleId);

        // Role.Name was just set from request.Name, so it cannot be null
        // But add defensive check for consistency
        if (string.IsNullOrEmpty(role.Name))
        {
            logger.LogError("Role updated but Name is null: {RoleId}", roleId);
            return Result.Failure<RoleResponse>(
                Error.Internal("Role.InvalidState", "Role name is missing after update"));
        }

        var userCount = await GetUserCountInRoleAsync(role.Name);
        var response = new RoleResponse(role.Id, role.Name, userCount);

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting role: {RoleId}", roleId);

        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleId}", roleId);
            return Result.Failure(
                Error.NotFound("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Defensive check for role name
        if (string.IsNullOrEmpty(role.Name))
        {
            logger.LogError("Role has null Name: {RoleId}", roleId);
            return Result.Failure(
                Error.Internal("Role.InvalidState", "Role name is missing"));
        }

        // Check if role has users
        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name);
        if (usersInRole.Any())
        {
            logger.LogWarning("Role deletion failed: Role {RoleId} has {UserCount} users", roleId, usersInRole.Count);
            return Result.Failure(
                Error.Conflict("Role.HasUsers", $"Cannot delete role '{role.Name}' because it has {usersInRole.Count} user(s). Remove users from role first."));
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Role deletion failed: {Errors}", errors);
            return Result.Failure(
                Error.Internal("Role.DeleteFailed", errors));
        }

        logger.LogInformation("Role deleted successfully: {RoleId}", roleId);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> AssignRoleToUserAsync(UserId userId, string roleName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Assigning role {RoleName} to user {UserId}", roleName, userId.Value);

        // Verify role exists
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleName}", roleName);
            return Result.Failure(
                Error.NotFound("Role.NotFound", $"Role '{roleName}' not found"));
        }

        // Verify user exists
        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId.Value);
            return Result.Failure(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        // Check if user already has role
        if (await userManager.IsInRoleAsync(user, roleName))
        {
            logger.LogWarning("User {UserId} already has role {RoleName}", userId.Value, roleName);
            return Result.Failure(
                Error.Conflict("User.AlreadyInRole", $"User already has the '{roleName}' role"));
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Role assignment failed: {Errors}", errors);
            return Result.Failure(
                Error.Internal("Role.AssignmentFailed", errors));
        }

        logger.LogInformation("Role {RoleName} assigned to user {UserId} successfully", roleName, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RemoveRoleFromUserAsync(UserId userId, string roleName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Removing role {RoleName} from user {UserId}", roleName, userId.Value);

        // Verify role exists
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleName}", roleName);
            return Result.Failure(
                Error.NotFound("Role.NotFound", $"Role '{roleName}' not found"));
        }

        // Verify user exists
        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId.Value);
            return Result.Failure(
                Error.NotFound("User.NotFound", $"User with ID {userId.Value} not found"));
        }

        // Check if user has role
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            logger.LogWarning("User {UserId} does not have role {RoleName}", userId.Value, roleName);
            return Result.Failure(
                Error.NotFound("User.NotInRole", $"User does not have the '{roleName}' role"));
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Role removal failed: {Errors}", errors);
            return Result.Failure(
                Error.Internal("Role.RemovalFailed", errors));
        }

        logger.LogInformation("Role {RoleName} removed from user {UserId} successfully", roleName, userId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<UserResponse>>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting users in role: {RoleName}", roleName);

        // Verify role exists
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            logger.LogWarning("Role not found: {RoleName}", roleName);
            return Result.Failure<IEnumerable<UserResponse>>(
                Error.NotFound("Role.NotFound", $"Role '{roleName}' not found"));
        }

        var users = await userManager.GetUsersInRoleAsync(roleName);

        var responses = new List<UserResponse>();
        foreach (var user in users)
        {
            // Skip users with null email (defensive - should never happen)
            if (string.IsNullOrEmpty(user.Email))
            {
                logger.LogWarning("Skipping user with null Email in role {RoleName}: {UserId}", roleName, user.Id);
                continue;
            }

            var roles = await userManager.GetRolesAsync(user);
            responses.Add(new UserResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                roles,
                user.CreatedAt,
                user.LastModifiedAt
            ));
        }

        return Result.Success<IEnumerable<UserResponse>>(responses);
    }

    /// <summary>
    /// Counts the users that have a role.
    /// </summary>
    private async Task<int> GetUserCountInRoleAsync(string roleName)
    {
        var users = await userManager.GetUsersInRoleAsync(roleName);
        return users.Count;
    }
}
