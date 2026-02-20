namespace UserManagement.Core.Enums;

/// <summary>
/// Represents the type of device used for authentication
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Unknown or unspecified device type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Web browser (Blazor WASM)
    /// </summary>
    Web = 1,

    /// <summary>
    /// Mobile device (MAUI iOS/Android)
    /// </summary>
    Mobile = 2,

    /// <summary>
    /// Desktop application (MAUI Windows/macOS/Linux)
    /// </summary>
    Desktop = 3
}
