namespace UserManagement.Infrastructure.Identity;

/// <summary>
/// Entity representing a user invitation stored in the database
/// </summary>
public sealed class UserInvitationEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string ActivationCode { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public DateTime InvitedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public bool IsAccepted { get; set; }
}
