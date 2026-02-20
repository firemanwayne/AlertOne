using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagement.Core.ValueObjects;

namespace UserManagement.Core.Tests.ValueObjects;

[TestClass]
public sealed class UserIdTests
{
    [TestMethod]
    public void New_GeneratesUniqueIds()
    {
        // Act
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Assert
        userId1.Value.Should().NotBe(userId2.Value);
        userId1.Should().NotBe(userId2);
    }

    [TestMethod]
    public void From_ValidGuid_ReturnsUserId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.From(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [TestMethod]
    public void From_EmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        Action act = () => UserId.From(emptyGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }

    [TestMethod]
    public void From_ValidGuidString_ReturnsUserId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var userId = UserId.From(guidString);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [TestMethod]
    public void From_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        const string emptyString = "";

        // Act
        Action act = () => UserId.From(emptyString);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }

    [TestMethod]
    public void From_WhitespaceString_ThrowsArgumentException()
    {
        // Arrange
        const string whitespaceString = "   ";

        // Act
        Action act = () => UserId.From(whitespaceString);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }

    [TestMethod]
    public void From_InvalidGuidString_ThrowsArgumentException()
    {
        // Arrange
        const string invalidGuid = "not-a-guid";

        // Act
        Action act = () => UserId.From(invalidGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID must be a valid GUID*");
    }

    [TestMethod]
    public void Equals_SameGuidValues_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = UserId.From(guid);
        var userId2 = UserId.From(guid);

        // Act & Assert
        userId1.Should().Be(userId2);
        (userId1 == userId2).Should().BeTrue();
        userId1.Equals(userId2).Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentGuidValues_ReturnsFalse()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Act & Assert
        userId1.Should().NotBe(userId2);
        (userId1 != userId2).Should().BeTrue();
        userId1.Equals(userId2).Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_SameGuidValues_ReturnsSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = UserId.From(guid);
        var userId2 = UserId.From(guid);

        // Act
        var hash1 = userId1.GetHashCode();
        var hash2 = userId2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [TestMethod]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId = UserId.From(guid);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }
}
