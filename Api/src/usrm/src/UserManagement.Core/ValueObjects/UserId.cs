namespace UserManagement.Core.ValueObjects;

/// <summary>
/// Represents a user identifier value object
/// </summary>
public sealed class UserId : IEquatable<UserId>
{
    /// <summary>
    /// Initializes a user identifier from a GUID.
    /// </summary>
    private UserId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the underlying GUID.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new unique user identifier
    /// </summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a user identifier from a GUID
    /// </summary>
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(value));
        }

        return new UserId(value);
    }

    /// <summary>
    /// Creates a user identifier from a string
    /// </summary>
    public static UserId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("User ID cannot be empty", nameof(value));
        }

        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException("User ID must be a valid GUID", nameof(value));
        }

        return From(guid);
    }

    /// <summary>
    /// Determines whether two user identifiers are equal.
    /// </summary>
    public bool Equals(UserId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value.Equals(other.Value);
    }

    /// <summary>
    /// Determines whether an object is an equal user identifier.
    /// </summary>
    public override bool Equals(object? obj) => obj is UserId userId && Equals(userId);
    /// <summary>
    /// Gets the hash code of the identifier.
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();
    /// <summary>
    /// Returns the identifier as a string.
    /// </summary>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Determines whether two user identifiers are equal.
    /// </summary>
    public static bool operator ==(UserId? left, UserId? right) => Equals(left, right);
    /// <summary>
    /// Determines whether two user identifiers are different.
    /// </summary>
    public static bool operator !=(UserId? left, UserId? right) => !Equals(left, right);
}
