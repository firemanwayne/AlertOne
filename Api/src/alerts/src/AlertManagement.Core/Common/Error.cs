namespace AlertManagement.Core.Common;

/// <summary>
/// Represents an error with a code, message, and type
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>
    /// Creates a validation error
    /// </summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>
    /// Creates a not found error
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Creates a conflict error
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Creates an unauthorized error
    /// </summary>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden error
    /// </summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Creates an internal error
    /// </summary>
    public static Error Internal(string code, string message) =>
        new(code, message, ErrorType.Internal);
}

/// <summary>
/// Represents the type of error
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// The request failed validation.
    /// </summary>
    Validation,
    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound,
    /// <summary>
    /// The request conflicts with the current state.
    /// </summary>
    Conflict,
    /// <summary>
    /// The caller is not authenticated.
    /// </summary>
    Unauthorized,
    /// <summary>
    /// The caller is not allowed to perform the operation.
    /// </summary>
    Forbidden,
    /// <summary>
    /// An unexpected server error occurred.
    /// </summary>
    Internal
}
