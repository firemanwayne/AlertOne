using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserManagement.Core.Common;

namespace UserManagement.Core.Tests.Common;

[TestClass]
public sealed class ResultTests
{
    [TestClass]
    public sealed class NonGenericResultTests
    {
        [TestMethod]
        public void Success_CreatesSuccessResult()
        {
            // Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().BeNull();
        }

        [TestMethod]
        public void Failure_WithError_CreatesFailureResult()
        {
            // Arrange
            var error = Error.Validation("Test.Error", "Test error message");

            // Act
            var result = Result.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Should().Be(error);
        }

        [TestMethod]
        public void IsFailure_OppositeOfIsSuccess()
        {
            // Arrange
            var successResult = Result.Success();
            var failureResult = Result.Failure(Error.Internal("Test.Error", "Test message"));

            // Assert
            successResult.IsFailure.Should().Be(!successResult.IsSuccess);
            failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
        }
    }

    [TestClass]
    public sealed class GenericResultTests
    {
        [TestMethod]
        public void Success_WithValue_CreatesSuccessResult()
        {
            // Arrange
            const string value = "test value";

            // Act
            var result = Result.Success(value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().BeNull();
            result.Value.Should().Be(value);
        }

        [TestMethod]
        public void Failure_WithError_CreatesFailureResult()
        {
            // Arrange
            var error = Error.NotFound("Test.NotFound", "Resource not found");

            // Act
            var result = Result.Failure<string>(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Should().Be(error);
        }

        [TestMethod]
        public void Value_OnSuccessResult_ReturnsValue()
        {
            // Arrange
            const int value = 42;
            var result = Result.Success(value);

            // Act
            var retrievedValue = result.Value;

            // Assert
            retrievedValue.Should().Be(value);
        }

        [TestMethod]
        public void Value_OnFailureResult_ThrowsInvalidOperationException()
        {
            // Arrange
            var error = Error.Internal("Test.Error", "Test message");
            var result = Result.Failure<string>(error);

            // Act
            Action act = () => { var value = result.Value; };

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot access value of a failed result.");
        }

        [TestMethod]
        public void Success_WithNullValue_CreatesSuccessResult()
        {
            // Arrange
            string? nullValue = null;

            // Act
            var result = Result.Success(nullValue);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [TestMethod]
        public void IsFailure_OppositeOfIsSuccess()
        {
            // Arrange
            var successResult = Result.Success("value");
            var failureResult = Result.Failure<string>(Error.Internal("Test.Error", "Test message"));

            // Assert
            successResult.IsFailure.Should().Be(!successResult.IsSuccess);
            failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
        }
    }

    [TestClass]
    public sealed class ErrorTests
    {
        [TestMethod]
        public void Validation_CreatesValidationError()
        {
            // Act
            var error = Error.Validation("Test.Validation", "Validation failed");

            // Assert
            error.Code.Should().Be("Test.Validation");
            error.Message.Should().Be("Validation failed");
            error.Type.Should().Be(ErrorType.Validation);
        }

        [TestMethod]
        public void NotFound_CreatesNotFoundError()
        {
            // Act
            var error = Error.NotFound("Test.NotFound", "Resource not found");

            // Assert
            error.Code.Should().Be("Test.NotFound");
            error.Message.Should().Be("Resource not found");
            error.Type.Should().Be(ErrorType.NotFound);
        }

        [TestMethod]
        public void Conflict_CreatesConflictError()
        {
            // Act
            var error = Error.Conflict("Test.Conflict", "Resource already exists");

            // Assert
            error.Code.Should().Be("Test.Conflict");
            error.Message.Should().Be("Resource already exists");
            error.Type.Should().Be(ErrorType.Conflict);
        }

        [TestMethod]
        public void Unauthorized_CreatesUnauthorizedError()
        {
            // Act
            var error = Error.Unauthorized("Test.Unauthorized", "Authentication failed");

            // Assert
            error.Code.Should().Be("Test.Unauthorized");
            error.Message.Should().Be("Authentication failed");
            error.Type.Should().Be(ErrorType.Unauthorized);
        }

        [TestMethod]
        public void Forbidden_CreatesForbiddenError()
        {
            // Act
            var error = Error.Forbidden("Test.Forbidden", "Access denied");

            // Assert
            error.Code.Should().Be("Test.Forbidden");
            error.Message.Should().Be("Access denied");
            error.Type.Should().Be(ErrorType.Forbidden);
        }

        [TestMethod]
        public void Internal_CreatesInternalError()
        {
            // Act
            var error = Error.Internal("Test.Internal", "Internal server error");

            // Assert
            error.Code.Should().Be("Test.Internal");
            error.Message.Should().Be("Internal server error");
            error.Type.Should().Be(ErrorType.Internal);
        }

        [TestMethod]
        public void Error_EqualityByValue()
        {
            // Arrange
            var error1 = Error.Validation("Test.Code", "Test message");
            var error2 = Error.Validation("Test.Code", "Test message");
            var error3 = Error.Validation("Different.Code", "Test message");

            // Assert
            error1.Should().Be(error2);
            error1.Should().NotBe(error3);
        }
    }
}
