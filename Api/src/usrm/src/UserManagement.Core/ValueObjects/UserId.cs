namespace UserManagement.Core.ValueObjects;

/// <summary>
/// Represents a user identifier value object
/// </summary>
public sealed class UserId : IEquatable<UserId>
{
    private UserId(Guid value)
    {
        Value = value;
    }

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

    public override bool Equals(object? obj) => obj is UserId userId && Equals(userId);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(UserId? left, UserId? right) => Equals(left, right);
    public static bool operator !=(UserId? left, UserId? right) => !Equals(left, right);
}
