using AlertOne.Core.Interfaces;

namespace AlertOne.Maui.Services;

/// <summary>
/// MAUI implementation of app version service using AppInfo
/// </summary>
internal sealed class MauiAppVersionService : IAppVersionService
{
    public string Version => AppInfo.Current.VersionString;

    public string Build => AppInfo.Current.BuildString;

    public string FullVersion => $"{Version} ({Build})";
}
