using AlertOne.Core.Common;
using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.DTOs.Responses;

namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for user settings operations
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Gets settings for a user
    /// </summary>
    Task<IApiResult<UserSettingsResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates settings for a user
    /// </summary>
    Task<IApiResult<UserSettingsResponse>> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default);
}
