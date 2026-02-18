using UserManagement.Core.Common;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Entities;

/// <summary>
/// Represents a user in the domain
/// </summary>
public sealed class User
{
    private readonly List<UserRole> _roles = [];

    private User(UserId id, Email email, string firstName, string lastName, DateTime createdAt)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = createdAt;
    }

    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public IReadOnlyList<UserRole> Roles => _roles.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public static Result<User> Create(Email email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<User>(
                Error.Validation("User.FirstNameEmpty", "First name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<User>(
                Error.Validation("User.LastNameEmpty", "Last name cannot be empty"));
        }

        if (firstName.Length > 100)
        {
            return Result.Failure<User>(
                Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters"));
        }

        if (lastName.Length > 100)
        {
            return Result.Failure<User>(
                Error.Validation("User.LastNameTooLong", "Last name must not exceed 100 characters"));
        }

        var user = new User(
            UserId.New(),
            email,
            firstName.Trim(),
            lastName.Trim(),
            DateTime.UtcNow);

        return Result.Success(user);
    }

    /// <summary>
    /// Updates the user's profile information
    /// </summary>
    public Result UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(
                Error.Validation("User.FirstNameEmpty", "First name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(
                Error.Validation("User.LastNameEmpty", "Last name cannot be empty"));
        }

        if (firstName.Length > 100)
        {
            return Result.Failure(
                Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters"));
        }

        if (lastName.Length > 100)
        {
            return Result.Failure(
                Error.Validation("User.LastNameTooLong", "Last name must not exceed 100 characters"));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Adds a role to the user
    /// </summary>
    public Result AddRole(UserRole role)
    {
        if (_roles.Contains(role))
        {
            return Result.Failure(
                Error.Conflict("User.RoleAlreadyExists", $"User already has the {role} role"));
        }

        _roles.Add(role);
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes a role from the user
    /// </summary>
    public Result RemoveRole(UserRole role)
    {
        if (!_roles.Contains(role))
        {
            return Result.Failure(
                Error.NotFound("User.RoleNotFound", $"User does not have the {role} role"));
        }

        _roles.Remove(role);

        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
