using AlertOne.Core.Common;
using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.DTOs.Responses;

namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for user management operations
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Gets all users
    /// </summary>
    Task<IApiResult<IEnumerable<UserResponse>>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<IApiResult<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user
    /// </summary>
    Task<IApiResult<UserResponse>> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user
    /// </summary>
    Task<IApiResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
