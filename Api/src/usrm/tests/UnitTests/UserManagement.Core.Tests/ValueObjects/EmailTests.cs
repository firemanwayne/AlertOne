using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Tests.ValueObjects;

[TestClass]
public sealed class EmailTests
{
    [TestMethod]
    public void Create_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        const string validEmail = "test@example.com";

        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Email is converted to lowercase
        result.Value.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [TestMethod]
    public void Create_ValidEmailWithPlus_ReturnsSuccess()
    {
        // Arrange
        const string validEmail = "test+tag@example.com";

        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Email is converted to lowercase
        result.Value.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [TestMethod]
    public void Create_ValidEmailWithSubdomain_ReturnsSuccess()
    {
        // Arrange
        const string validEmail = "test@mail.example.com";

        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Email is converted to lowercase
        result.Value.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [TestMethod]
    public void Create_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        const string emptyEmail = "";

        // Act
        var result = Email.Create(emptyEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Empty");
        }
    }

    [TestMethod]
    public void Create_NullEmail_ReturnsFailure()
    {
        // Arrange
        string? nullEmail = null;

        // Act
        var result = Email.Create(nullEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Empty");
        }
    }

    [TestMethod]
    public void Create_WhitespaceEmail_ReturnsFailure()
    {
        // Arrange
        const string whitespaceEmail = "   ";

        // Act
        var result = Email.Create(whitespaceEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Empty");
        }
    }

    [TestMethod]
    public void Create_EmailWithoutAtSign_ReturnsFailure()
    {
        // Arrange
        const string invalidEmail = "testexample.com";

        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Invalid");
        }
    }

    [TestMethod]
    public void Create_EmailWithoutDomain_ReturnsFailure()
    {
        // Arrange
        const string invalidEmail = "test@";

        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Invalid");
        }
    }

    [TestMethod]
    public void Create_EmailWithoutLocalPart_ReturnsFailure()
    {
        // Arrange
        const string invalidEmail = "@example.com";

        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Invalid");
        }
    }

    [TestMethod]
    public void Create_EmailTooLong_ReturnsFailure()
    {
        // Arrange
        var longLocalPart = new string('a', 248); // 248 + "@test.com" = 257 characters (exceeds 256)
        var tooLongEmail = $"{longLocalPart}@test.com";

        // Act
        var result = Email.Create(tooLongEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.TooLong");
        }
    }

    [TestMethod]
    public void Create_EmailWithSpaces_ReturnsFailure()
    {
        // Arrange
        const string invalidEmail = "test user@example.com";

        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        if (result.Error is not null)
        {
            result.Error.Code.Should().Be("Email.Invalid");
        }
    }

    [TestMethod]
    public void Equals_SameEmailValues_ReturnsTrue()
    {
        // Arrange
        const string emailValue = "test@example.com";
        var email1 = Email.Create(emailValue).Value;
        var email2 = Email.Create(emailValue).Value;

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentEmailValues_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com").Value;
        var email2 = Email.Create("test2@example.com").Value;

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [TestMethod]
    public void ToString_ReturnsEmailValue()
    {
        // Arrange
        const string emailValue = "test@example.com";
        var email = Email.Create(emailValue).Value;

        // Act
        var result = email.ToString();

        // Assert
        // Email is stored in lowercase
        result.Should().Be(emailValue.ToLowerInvariant());
    }
}
