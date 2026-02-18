namespace UserManagementApps.Core.Common;

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result : IApiResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error if the operation failed
    /// </summary>
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException("Success result cannot have an error", nameof(error));
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentException("Failure result must have an error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a success result
    /// </summary>
    public static IApiResult Success() => new Result(true, null);

    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static IApiResult Failure(Error error) => new Result(false, error);
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public sealed class Result<T> : Result, IApiResult<T>
{
    private readonly T? _value;

    /// <summary>
    /// Gets the value if the operation was successful
    /// </summary>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access value of a failed result");
            }

            return _value!;
        }
    }

    private Result(bool isSuccess, T? value, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a success result with a value
    /// </summary>
    public static IApiResult<T> Success(T value) => new Result<T>(true, value, null);

    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static new IApiResult<T> Failure(Error error) => new Result<T>(false, default, error);
}
