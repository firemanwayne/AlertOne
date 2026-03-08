using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for user settings operations
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Gets settings for a user
    /// </summary>
    Task<Result<UserSettingsResponse>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates settings for a user
    /// </summary>
    Task<Result<UserSettingsResponse>> UpdateAsync(UserId userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates default settings for a new user
    /// </summary>
    Task<Result<UserSettingsResponse>> CreateDefaultAsync(UserId userId, CancellationToken cancellationToken = default);
}
