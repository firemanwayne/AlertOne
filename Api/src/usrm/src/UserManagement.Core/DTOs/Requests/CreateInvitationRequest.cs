namespace UserManagement.Core.DTOs.Requests;

/// <summary>
/// Request to create a new user invitation
/// </summary>
public sealed record CreateInvitationRequest(
    string Email,
    string InvitedBy);
