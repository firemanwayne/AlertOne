namespace AlertOne.Core.DTOs.Requests;

/// <summary>
/// Request to accept a user invitation and complete registration
/// </summary>
public sealed record AcceptInvitationRequest(
    string ActivationCode,
    string FirstName,
    string LastName,
    string Password);
