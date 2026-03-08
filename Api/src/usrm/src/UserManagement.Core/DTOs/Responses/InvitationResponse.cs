namespace UserManagement.Core.DTOs.Responses;

/// <summary>
/// Response model for invitation information
/// </summary>
public sealed record InvitationResponse(
    string Id,
    string Email,
    string InvitedBy,
    DateTime InvitedAt,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    bool IsAccepted,
    bool IsExpired,
    string Status);
