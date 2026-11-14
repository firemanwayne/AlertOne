using AlertOne.Core.Interfaces;

namespace AlertOne.Maui.Services;

/// <summary>
/// MAUI implementation of app version service using AppInfo
/// </summary>
internal sealed class MauiAppVersionService : IAppVersionService
{
    /// <inheritdoc />
    public string Version => AppInfo.Current.VersionString;

    /// <inheritdoc />
    public string Build => AppInfo.Current.BuildString;

    /// <inheritdoc />
    public string FullVersion => $"{Version} ({Build})";
}
