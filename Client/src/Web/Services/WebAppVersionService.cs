using System.Reflection;
using AlertOne.Core.Interfaces;

namespace AlertOne.Web.Services;

/// <summary>
/// Web/WASM implementation of app version service using assembly version
/// </summary>
internal sealed class WebAppVersionService : IAppVersionService
{
    private readonly string _version;
    private readonly string _build;

    public WebAppVersionService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        // Use informational version if available (set by CI/CD pipeline)
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrEmpty(infoVersion))
        {
            // Parse informational version (e.g., "1.0.0+build.123" or "1.0.0-preview.1+build.123")
            var plusIndex = infoVersion.IndexOf('+');
            if (plusIndex > 0)
            {
                _version = infoVersion[..plusIndex];
                _build = infoVersion[(plusIndex + 1)..].Replace("build.", "");
            }
            else
            {
                _version = infoVersion;
                _build = version?.Revision.ToString() ?? "0";
            }
        }
        else if (version is not null)
        {
            _version = $"{version.Major}.{version.Minor}.{version.Build}";
            _build = version.Revision.ToString();
        }
        else
        {
            _version = "1.0.0";
            _build = "0";
        }
    }

    public string Version => _version;

    public string Build => _build;

    public string FullVersion => $"{Version} ({Build})";
}
