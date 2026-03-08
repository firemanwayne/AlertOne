using UserManagement.Core.Common;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Entities;

/// <summary>
/// Represents a user invitation in the domain
/// </summary>
public sealed class UserInvitation
{
    private UserInvitation(string id, Email email, string activationCode, string invitedBy, DateTime invitedAt, DateTime expiresAt)
    {
        Id = id;
        Email = email;
        ActivationCode = activationCode;
        InvitedBy = invitedBy;
        InvitedAt = invitedAt;
        ExpiresAt = expiresAt;
        IsAccepted = false;
        AcceptedAt = null;
    }

    public string Id { get; private set; }
    public Email Email { get; private set; }
    public string ActivationCode { get; private set; }
    public string InvitedBy { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public bool IsAccepted { get; private set; }

    /// <summary>
    /// Checks if the invitation has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if the invitation is valid (not expired and not already accepted)
    /// </summary>
    public bool IsValid => !IsExpired && !IsAccepted;

    /// <summary>
    /// Creates a new user invitation
    /// </summary>
    public static Result<UserInvitation> Create(Email email, string invitedBy, int expirationDays = 7)
    {
        if (string.IsNullOrWhiteSpace(invitedBy))
        {
            return Result.Failure<UserInvitation>(
                Error.Validation("Invitation.InvitedByEmpty", "Inviter name cannot be empty"));
        }

        if (expirationDays <= 0)
        {
            return Result.Failure<UserInvitation>(
                Error.Validation("Invitation.InvalidExpiration", "Expiration days must be greater than zero"));
        }

        var invitation = new UserInvitation(
            Guid.NewGuid().ToString(),
            email,
            GenerateActivationCode(),
            invitedBy.Trim(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(expirationDays));

        return Result.Success(invitation);
    }

    /// <summary>
    /// Marks the invitation as accepted
    /// </summary>
    public Result Accept()
    {
        if (IsAccepted)
        {
            return Result.Failure(
                Error.Conflict("Invitation.AlreadyAccepted", "This invitation has already been accepted"));
        }

        if (IsExpired)
        {
            return Result.Failure(
                Error.Validation("Invitation.Expired", "This invitation has expired"));
        }

        IsAccepted = true;
        AcceptedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Regenerates the activation code and extends expiration
    /// </summary>
    public Result Resend(string resendBy, int expirationDays = 7)
    {
        if (IsAccepted)
        {
            return Result.Failure(
                Error.Conflict("Invitation.AlreadyAccepted", "Cannot resend an already accepted invitation"));
        }

        if (string.IsNullOrWhiteSpace(resendBy))
        {
            return Result.Failure(
                Error.Validation("Invitation.ResendByEmpty", "Resender name cannot be empty"));
        }

        ActivationCode = GenerateActivationCode();
        InvitedBy = resendBy.Trim();
        InvitedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);

        return Result.Success();
    }

    /// <summary>
    /// Generates a 6-digit activation code
    /// </summary>
    private static string GenerateActivationCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
