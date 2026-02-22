using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;

namespace UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for user invitation operations
/// </summary>
public interface IInvitationService
{
    /// <summary>
    /// Creates a new user invitation and sends an email
    /// </summary>
    Task<Result<InvitationResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invitations
    /// </summary>
    Task<Result<IEnumerable<InvitationResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by its activation code
    /// </summary>
    Task<Result<InvitationResponse>> GetByCodeAsync(string activationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by its unique identifier
    /// </summary>
    Task<Result<InvitationResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an activation code
    /// </summary>
    Task<Result<InvitationResponse>> ValidateCodeAsync(string activationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Accepts an invitation and creates the user account
    /// </summary>
    Task<Result<UserResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an invitation
    /// </summary>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends an invitation email with a new activation code
    /// </summary>
    Task<Result<InvitationResponse>> ResendAsync(string id, string resendBy, CancellationToken cancellationToken = default);
}
