namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request to resend a user invitation
/// </summary>
public sealed record ResendInvitationRequest(string ResendBy);
