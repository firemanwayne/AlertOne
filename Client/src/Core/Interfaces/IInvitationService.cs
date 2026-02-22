using AlertOne.Core.Common;
using AlertOne.Core.DTOs.Requests;
using AlertOne.Core.DTOs.Responses;

namespace AlertOne.Core.Interfaces;

/// <summary>
/// Service for user invitation operations
/// </summary>
public interface IInvitationService
{
    /// <summary>
    /// Gets all invitations
    /// </summary>
    Task<IApiResult<IEnumerable<InvitationResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by ID
    /// </summary>
    Task<IApiResult<InvitationResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new invitation
    /// </summary>
    Task<IApiResult<InvitationResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an activation code
    /// </summary>
    Task<IApiResult<InvitationResponse>> ValidateCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Accepts an invitation and creates the user account
    /// </summary>
    Task<IApiResult<UserResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an invitation
    /// </summary>
    Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends an invitation
    /// </summary>
    Task<IApiResult<InvitationResponse>> ResendAsync(string id, ResendInvitationRequest request, CancellationToken cancellationToken = default);
}
