namespace AlertOne.Core.Common;

/// <summary>
/// Represents the result of an API operation without a return value
/// </summary>
public interface IApiResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the error if the operation failed
    /// </summary>
    Error? Error { get; }
}

/// <summary>
/// Represents the result of an API operation with a return value
/// </summary>
public interface IApiResult<out T> : IApiResult
{
    /// <summary>
    /// Gets the value if the operation was successful
    /// </summary>
    T Value { get; }
}
