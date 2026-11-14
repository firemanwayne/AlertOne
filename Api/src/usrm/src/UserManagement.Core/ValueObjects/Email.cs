using System.Text.RegularExpressions;
using UserManagement.Core.Common;

namespace UserManagement.Core.ValueObjects;

/// <summary>
/// Represents an email address value object
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    /// <summary>
    /// The compiled email address validation expression.
    /// </summary>
    private static readonly Regex _emailRegex = EmailParser();

    /// <summary>
    /// Initializes an email address from an already validated, lowercased value.
    /// </summary>
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the email address.
    /// </summary>
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

    /// <summary>
    /// Determines whether two email addresses are equal.
    /// </summary>
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

    /// <summary>
    /// Determines whether an object is an equal email address.
    /// </summary>
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    /// <summary>
    /// Gets the hash code of the email address.
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();
    /// <summary>
    /// Returns the email address.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Determines whether two email addresses are equal.
    /// </summary>
    public static bool operator ==(Email? left, Email? right) => Equals(left, right);
    /// <summary>
    /// Determines whether two email addresses are different.
    /// </summary>
    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

    /// <summary>
    /// Generated email address validation expression.
    /// </summary>
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EmailParser();
}
