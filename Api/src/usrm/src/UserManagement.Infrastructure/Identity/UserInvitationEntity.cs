namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Entity representing a user invitation stored in the database
/// </summary>
public sealed class UserInvitationEntity
{
    /// <summary>
    /// Gets or sets the invitation identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Gets or sets the invited email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the activation code sent to the invitee.
    /// </summary>
    public string ActivationCode { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets who sent the invitation.
    /// </summary>
    public string InvitedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the UTC time the invitation was sent.
    /// </summary>
    public DateTime InvitedAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// Gets or sets the UTC time the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the invitation was accepted.
    /// </summary>
    public bool IsAccepted { get; set; }
}
