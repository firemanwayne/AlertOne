using System.Text.RegularExpressions;
using UserManagement.Core.Common;

namespace UserManagement.Core.ValueObjects;

/// <summary>
/// Represents an email address value object
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex _emailRegex = EmailParser();

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Creates an Email value object
    /// </summary>
    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(
                Error.Validation("Email.Empty", "Email cannot be empty"));
        }

        email = email.Trim();

        if (email.Length > 256)
        {
            return Result.Failure<Email>(
                Error.Validation("Email.TooLong", "Email must not exceed 256 characters"));
        }

        if (!_emailRegex.IsMatch(email))
        {
            return Result.Failure<Email>(
                Error.Validation("Email.Invalid", "Email format is invalid"));
        }

        return Result.Success(new Email(email.ToLowerInvariant()));
    }

    public bool Equals(Email? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(Email? left, Email? right) => Equals(left, right);
    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EmailParser();
}
