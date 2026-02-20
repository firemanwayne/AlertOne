using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagement.Core.Entities;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Tests.Entities;

[TestClass]
public sealed class UserTests
{
    [TestMethod]
    public void Create_ValidParameters_ReturnsSuccessWithUser()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "John";
        const string lastName = "Doe";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(email);
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.Roles.Should().BeEmpty();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Value.LastModifiedAt.Should().BeNull();
    }

    [TestMethod]
    public void Create_FirstNameWithWhitespace_TrimsFirstName()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "  John  ";
        const string lastName = "Doe";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
    }

    [TestMethod]
    public void Create_LastNameWithWhitespace_TrimsLastName()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "John";
        const string lastName = "  Doe  ";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LastName.Should().Be("Doe");
    }

    [TestMethod]
    public void Create_EmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "";
        const string lastName = "Doe";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.FirstNameEmpty");
        }
    }

    [TestMethod]
    public void Create_WhitespaceFirstName_ReturnsFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "   ";
        const string lastName = "Doe";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.FirstNameEmpty");
        }
    }

    [TestMethod]
    public void Create_EmptyLastName_ReturnsFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "John";
        const string lastName = "";

        // Act
        var result = User.Create(email, firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.LastNameEmpty");
        }
    }

    [TestMethod]
    public void Create_FirstNameTooLong_ReturnsFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var longFirstName = new string('a', 101);
        const string lastName = "Doe";

        // Act
        var result = User.Create(email, longFirstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.FirstNameTooLong");
        }
    }

    [TestMethod]
    public void Create_LastNameTooLong_ReturnsFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string firstName = "John";
        var longLastName = new string('a', 101);

        // Act
        var result = User.Create(email, firstName, longLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.LastNameTooLong");
        }
    }

    [TestMethod]
    public void UpdateProfile_ValidParameters_UpdatesUserAndReturnsSuccess()
    {
        // Arrange
        var user = CreateTestUser();
        const string newFirstName = "Jane";
        const string newLastName = "Smith";

        // Act
        var result = user.UpdateProfile(newFirstName, newLastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.LastModifiedAt.Should().NotBeNull();
        user.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void UpdateProfile_FirstNameWithWhitespace_TrimsFirstName()
    {
        // Arrange
        var user = CreateTestUser();
        const string newFirstName = "  Jane  ";
        const string newLastName = "Smith";

        // Act
        var result = user.UpdateProfile(newFirstName, newLastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("Jane");
    }

    [TestMethod]
    public void UpdateProfile_EmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        const string newFirstName = "";
        const string newLastName = "Smith";

        // Act
        var result = user.UpdateProfile(newFirstName, newLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.FirstNameEmpty");
        }
    }

    [TestMethod]
    public void UpdateProfile_EmptyLastName_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        const string newFirstName = "Jane";
        const string newLastName = "";

        // Act
        var result = user.UpdateProfile(newFirstName, newLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.LastNameEmpty");
        }
    }

    [TestMethod]
    public void UpdateProfile_FirstNameTooLong_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var longFirstName = new string('a', 101);
        const string newLastName = "Smith";

        // Act
        var result = user.UpdateProfile(longFirstName, newLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.FirstNameTooLong");
        }
    }

    [TestMethod]
    public void UpdateProfile_LastNameTooLong_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        const string newFirstName = "Jane";
        var longLastName = new string('a', 101);

        // Act
        var result = user.UpdateProfile(newFirstName, longLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.LastNameTooLong");
        }
    }

    [TestMethod]
    public void AddRole_NewRole_AddsRoleAndReturnsSuccess()
    {
        // Arrange
        var user = CreateTestUser();
        const UserRole newRole = UserRole.Admin;

        // Act
        var result = user.AddRole(newRole);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Roles.Should().Contain(newRole);
        user.LastModifiedAt.Should().NotBeNull();
    }

    [TestMethod]
    public void AddRole_ExistingRole_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        const UserRole role = UserRole.User;
        user.AddRole(role);

        // Act
        var result = user.AddRole(role);

        // Assert
        result.IsFailure.Should().BeTrue();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.RoleAlreadyExists");
        }
    }

    [TestMethod]
    public void RemoveRole_ExistingRole_RemovesRoleAndReturnsSuccess()
    {
        // Arrange
        var user = CreateTestUser();
        const UserRole role = UserRole.User;
        user.AddRole(role);

        // Act
        var result = user.RemoveRole(role);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Roles.Should().NotContain(role);
        user.LastModifiedAt.Should().NotBeNull();
    }

    [TestMethod]
    public void RemoveRole_NonExistentRole_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        const UserRole role = UserRole.Admin;

        // Act
        var result = user.RemoveRole(role);

        // Assert
        result.IsFailure.Should().BeTrue();

        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("User.RoleNotFound");
        }
    }

    [TestMethod]
    public void Roles_ReturnsReadOnlyList()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddRole(UserRole.User);

        // Act
        var roles = user.Roles;

        // Assert
        roles.Should().BeAssignableTo<IReadOnlyList<UserRole>>();
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        const string firstName = "John";
        const string lastName = "Doe";

        return User.Create(email, firstName, lastName).Value;
    }
}
