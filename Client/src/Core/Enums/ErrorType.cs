namespace AlertOne.Core.Enums;

/// <summary>
/// Represents the type of error that occurred
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Validation error (400 Bad Request)
    /// </summary>
    Validation = 0,

    /// <summary>
    /// Resource not found (404 Not Found)
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// Conflict error (409 Conflict)
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Unauthorized access (401 Unauthorized)
    /// </summary>
    Unauthorized = 3,

    /// <summary>
    /// Forbidden access (403 Forbidden)
    /// </summary>
    Forbidden = 4,

    /// <summary>
    /// Internal server error (500 Internal Server Error)
    /// </summary>
    Internal = 5
}
