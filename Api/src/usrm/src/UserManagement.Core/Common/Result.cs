namespace UserManagement.Core.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a result and enforces that failures carry an error.
    /// </summary>
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
    /// <summary>
    /// Gets the error when the operation failed; otherwise null.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);
    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result carrying a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    /// <summary>
    /// Creates a failed result of the given value type.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
public class Result<TValue> : Result
{
    /// <summary>
    /// The result value (only set when successful).
    /// </summary>
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a typed result.
    /// </summary>
    protected internal Result(TValue? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value; throws if the result is a failure.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Implicitly converts a value into a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
