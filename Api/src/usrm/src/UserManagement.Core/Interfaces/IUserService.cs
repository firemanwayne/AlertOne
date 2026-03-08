using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user (admin operation)
    /// </summary>
    Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    Task<Result<UserResponse>> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    Task<Result<UserResponse>> GetUserByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's information
    /// </summary>
    Task<Result<UserResponse>> UpdateUserAsync(UserId userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user
    /// </summary>
    Task<Result> DeleteUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users
    /// </summary>
    Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
