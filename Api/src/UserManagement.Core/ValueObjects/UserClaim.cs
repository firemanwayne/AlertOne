namespace UserManagement.Core.ValueObjects;

/// <summary>
/// Represents a user claim with type and value
/// </summary>
public sealed record UserClaim(string ClaimType, string ClaimValue);
